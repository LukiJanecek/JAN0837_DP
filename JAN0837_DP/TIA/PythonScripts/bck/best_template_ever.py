# this code works properly

# tia_openness_py.py
# Requires: pip install pythonnet
# Run: python -Xutf8 yourscript.py

import os, sys, glob, tempfile
import clr
from pathlib import Path
from typing import Optional, Tuple

# ==============
#  Loader TIA API (V19) – dohledá všechny závislosti přes AssemblyResolve
# ==============
def init_tia_api(tia_root: Optional[str] = None, public_api_subdir: str = "PublicAPI", version: str = "V19"):
    r"""
    Inicializace prostředí pro TIA Openness:
      - přidá PublicAPI\VXX a Bin do DLL path
      - registruje AssemblyResolve pro dohledání chybějících DLL v celé instalaci
      - provede AddReference("Siemens.Engineering")
    """
    import clr  # až teď
    import System
    from System.Reflection import Assembly, AssemblyName
    from System import ResolveEventHandler

    # 1) Najdi kořen instalace TIA
    if tia_root is None:
        tia_root = rf"C:\Program Files\Siemens\Automation\Portal {version}"
    tia_root_path = Path(tia_root)
    if not tia_root_path.is_dir():
        raise FileNotFoundError(f"TIA root not found: {tia_root_path}")

    tia_public = tia_root_path / public_api_subdir / version
    tia_bin = tia_root_path / "Bin"
    if not tia_public.is_dir():
        raise FileNotFoundError(f"PublicAPI dir not found: {tia_public}")
    if not tia_bin.is_dir():
        raise FileNotFoundError(f"Bin dir not found: {tia_bin}")

    # 2) DLL paths (nativní i managed)
    if hasattr(os, "add_dll_directory"):
        os.add_dll_directory(str(tia_public))
        os.add_dll_directory(str(tia_bin))
    sys.path.extend([str(tia_public), str(tia_bin)])

    # 3) AssemblyResolve – dohledá libovolnou TIA *.dll (včetně Siemens.Engineering.Contract)
    _SEARCH_ROOTS = [tia_public, tia_bin, tia_root_path]
    _CACHE: dict[str, Path] = {}

    def _resolve(sender, args):
        name = AssemblyName(args.Name).Name  # např. "Siemens.Engineering.Contract"
        p = _CACHE.get(name)
        if p and p.is_file():
            return Assembly.LoadFrom(str(p))
        # Nejprve přímý pokus v PublicAPI a Bin
        for root in _SEARCH_ROOTS[:2]:
            cand = root / f"{name}.dll"
            if cand.is_file():
                _CACHE[name] = cand
                return Assembly.LoadFrom(str(cand))
        # Fallback – rekurzivně v celé instalaci
        for cand in _SEARCH_ROOTS[-1].rglob(f"{name}.dll"):
            _CACHE[name] = cand
            return Assembly.LoadFrom(str(cand))
        return None

    resolver_delegate = ResolveEventHandler(_resolve)
    System.AppDomain.CurrentDomain.add_AssemblyResolve(resolver_delegate)
    # Udržet silnou referenci, aby GC neodregistroval handler
    globals()["_TIA_RESOLVER_DELEGATE"] = resolver_delegate

    # 4) Načti hlavní assembly
    clr.AddReference("Siemens.Engineering")
    # sanity import (ať to failne hned, když něco chybí)
    from Siemens.Engineering import TiaPortal, TiaPortalMode  # noqa: F401

    return str(tia_public), str(tia_bin)

def open_or_attach_project(project_path: str, with_ui: bool = True, tia_root: Optional[str] = None, version: str = "V19") -> Tuple["TiaPortal", "Project"]:    
    r"""
    Python ekvivalent:
    - Ověří existenci .ap* souboru projektu
    - Attach na běžící TIA s otevřeným projektem stejné cesty
    - Jinak spustí TIA (UI/bez UI) a projekt otevře
    Vrací (tia, project). Nezapomeň pak na tia.Dispose().
    """
    if not project_path or not os.path.isfile(project_path):
        raise FileNotFoundError(f"TIA project file not found: {project_path}")

    # Init TIA API (důležité před prvním importem Siemens.*)
    init_tia_api(tia_root=tia_root, version=version)

    from Siemens.Engineering import TiaPortal, TiaPortalMode
    from System.IO import FileInfo

    # Attach na běžící procesy
    for p in TiaPortal.GetProcesses():
        try:
            pp = p.ProjectPath
            if pp and pp.FullName and str(pp.FullName).lower() == os.path.abspath(project_path).lower():
                attached = p.Attach()
                return attached, attached.Projects[0]
        except Exception:
            # Může padnout pokud proces nemá projekt – ignorujeme
            pass

    # Start nové instance
    mode = TiaPortalMode.WithUserInterface if with_ui else TiaPortalMode.WithoutUserInterface
    tia = TiaPortal(mode)
    tia.Projects.Open(FileInfo(project_path))
    return tia, tia.Projects[0]

def get_plc_software(project: "Project", device_name: Optional[str] = None) -> "PlcSoftware":
    """
    Najde PlcSoftware z projektu.
    Používá PŘÍMO FQN:
      - Siemens.Engineering.HW.Features.SoftwareContainer
      - Siemens.Engineering.SW.PLC.PlcSoftware
    Nejdřív horní úroveň DeviceItems, pak rekurzivní fallback.
    """
    import System
    from System.Reflection import Assembly

    asm = Assembly.Load("Siemens.Engineering")

    SC_FQN  = "Siemens.Engineering.HW.Features.SoftwareContainer"
    PLC_FQN = "Siemens.Engineering.SW.PlcSoftware"

    sc_type  = asm.GetType(SC_FQN,  False)  # žádný autothrow
    plc_type = asm.GetType(PLC_FQN, False)

    if sc_type is None or plc_type is None:
        # Diagnostika: ukaž, co skutečně je načtené
        seen = []
        for a in System.AppDomain.CurrentDomain.GetAssemblies():
            try:
                n = a.GetName().Name
                if n and n.startswith("Siemens.Engineering"):
                    for t in a.GetTypes():
                        if t.Name in ("SoftwareContainer", "PlcSoftware"):
                            seen.append(f"{t.FullName} in {n}")
            except Exception:
                pass
        msg = (
            f"Chybí typy (strict FQN): "
            f"{'OK' if sc_type else 'MISSING'}={SC_FQN}, "
            f"{'OK' if plc_type else 'MISSING'}={PLC_FQN}. "
            f"Nalezené kandidáty: { ' | '.join(seen) or '— nic —' }"
        )
        raise ImportError(msg)

    def _is_plc_software(obj) -> bool:
        if obj is None:
            return False
        try:
            return plc_type.IsAssignableFrom(obj.GetType())
        except Exception:
            return False

    def _match(dev_name: str) -> bool:
        return (not device_name) or (str(dev_name).lower() == str(device_name).lower())

    # 1) Horní úroveň (odpovídá tvému C# vzoru)
    for dev in project.Devices:
        for di in dev.DeviceItems:
            sc = _get_service_generic(di, sc_type)
            sw = getattr(sc, "Software", None) if sc is not None else None
            if _is_plc_software(sw) and _match(dev.Name):
                return sw

    # 2) Rekurzivní fallback (kdyby CPU bylo zanořené)
    for dev in project.Devices:
        stack = list(getattr(dev, "DeviceItems", []))
        while stack:
            di = stack.pop()
            sc = _get_service_generic(di, sc_type)
            sw = getattr(sc, "Software", None) if sc is not None else None
            if _is_plc_software(sw) and _match(dev.Name):
                return sw
            try:
                for child in di.DeviceItems:
                    stack.append(child)
            except Exception:
                pass

    raise RuntimeError("PLC software not found in project.")    
    
def _get_service_generic(device_item, service_type):
    """
    Zavolá device_item.GetService[T]() s T=service_type (System.Type).
    1) Zkusíme přímo pythonnet generika: device_item.GetService[service_type]()
    2) Fallback: reflexe přes CLR typ a MakeGenericMethod
    """
    if service_type is None or device_item is None:
        return None

    # 1) pythonnet generika (pokud to verze dovolí)
    try:
        return device_item.GetService[service_type]()
    except Exception:
        pass

    # 2) Reflexe: vezmi CLR typ objektu (ne pythoní type(...))
    from System.Reflection import BindingFlags
    clr_type = device_item.GetType()  # <- DŮLEŽITÉ: CLR typ
    try:
        methods = clr_type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
    except Exception:
        methods = clr_type.GetMethods()  # fallback, většinou stačí

    # Najdi generickou metodu GetService<T>()
    candidate = None
    for m in methods:
        try:
            if m.Name == "GetService" and m.IsGenericMethodDefinition:
                candidate = m
                break
        except Exception:
            continue

    if candidate is None:
        return None

    try:
        mi = candidate.MakeGenericMethod(service_type)
        return mi.Invoke(device_item, None)
    except Exception:
        return None

def _write_temp_scl(filename_hint: str, content: str) -> str:
    tmp = os.path.join(tempfile.gettempdir(), filename_hint)
    with open(tmp, "w", encoding="utf-8") as f:
        f.write(content)
    return tmp

def create_or_replace_simple_db(plc: "PlcSoftware", db_name: str, optimized: bool = True) -> None:
    """
    Vytvoří/nahradí jednoduchý DB přes SCL externí zdroj.
    ČISTÉ SCL (bez komentářů, bez diakritiky), jen tři proměnné: Speed, Count, Enabled.
    """
    # Import pro PlcBlock – v Openness je ve SW.Blocks
    from Siemens.Engineering.SW.Blocks import PlcBlock

    # 1) Sestav SCL bez diakritiky a bez komentaru
    attr = "{ S7_Optimized_Access := 'TRUE' }" if optimized else "{ S7_Optimized_Access := 'FALSE' }"
    db_scl = f"""DATA_BLOCK {db_name}
{attr}
VERSION : 0.1
  VAR
    Speed       : Real := 0.0;
    Count       : DInt := 0;
    Enabled     : Bool := FALSE;
  END_VAR
BEGIN
END_DATA_BLOCK
"""

    # 2) Smazat existujici DB stejneho jmena (pokud existuje)
    for b in list(plc.BlockGroup.Blocks):
        if isinstance(b, PlcBlock) and str(b.Name) == db_name:
            b.Delete()
            break

    # 3) External source – vycistit kolidujici zdroj a importovat
    src_group = plc.ExternalSourceGroup

    # normalizace nazvu souboru, at nevznikne "DB_DB_*.scl"
    file_stub = db_name
    if file_stub.upper().startswith("DB_"):
        file_stub = file_stub[3:]
    src_name = f"DB_{file_stub}.scl"

    # helper na smazani kolidujicich external sources
    def _delete_existing_external_source(src_group_local, name: str):
        try:
            for es in list(src_group_local.ExternalSources):
                if str(es.Name) == name:
                    es.Delete()
        except Exception:
            pass

    _delete_existing_external_source(src_group, src_name)

    # zapis souboru – pouzijeme ASCII-compat, aby vubec nemohla proklouznout diakritika
    import os, tempfile
    tmpfile = os.path.join(tempfile.gettempdir(), src_name)
    with open(tmpfile, "w", encoding="utf-8") as f:
        # obsah je ASCII only; utf-8 je OK
        f.write(db_scl)

    try:
        src = src_group.ExternalSources.CreateFromFile(src_name, tmpfile)
        src.GenerateBlocksFromSource()
        # drzet projekt cisty
        try:
            src.Delete()
        except Exception:
            pass
    finally:
        try:
            os.remove(tmpfile)
        except OSError:
            pass


def create_or_replace_udt(plc: "PlcSoftware", type_name: str, fields: Optional[list] = None) -> None:
    """
    Vytvoří/nahradí PLC datový typ (UDT) přes TYPE/END_TYPE.
    """
    from Siemens.Engineering.SW.Blocks import PlcBlock

    fields = fields or [("Speed", "Real", "0.0"),
                        ("Count", "DInt", "0"),
                        ("Enabled", "Bool", "FALSE")]

    lines = []
    for (name, typ, init) in fields:
        t = str(typ).upper()
        if t in ("DATE_AND_TIME", "DATEANDTIME", "DATE_TIME", "DATETIME", "DATE-AND-TIME"):
            t = "DATE_AND_TIME"
        # sjednocení literálů pro DTL/DATE_AND_TIME (nepovinné, ale praktické)
        if t == "DTL" and isinstance(init, str) and init.startswith("DT#"):
            init = "DTL#" + init[3:]
        if t == "DATE_AND_TIME" and isinstance(init, str) and init.startswith("DTL#"):
            init = "DT#" + init[4:]

        if init is None or str(init) == "":
            lines.append(f"  {name:12s} : {t};")
        else:
            lines.append(f"  {name:12s} : {t} := {init};")
    body = "\n".join(lines)

    type_scl = f"""TYPE {type_name}
        VERSION : 0.1
        STRUCT
        {body}
        END_STRUCT
        END_TYPE
        """

    # smazat existující UDT stejného jména
    for b in list(plc.BlockGroup.Blocks):
        if isinstance(b, PlcBlock) and str(b.Name) == type_name:
            b.Delete()
            break

    src_group = plc.ExternalSourceGroup
    src_name = f"TYPE_{type_name}.scl"

    # 💥 důležité: odstranit případný zbylý External Source stejného jména
    _delete_existing_external_source(src_group, src_name)

    tmpfile = _write_temp_scl(src_name, type_scl)
    try:
        src = src_group.ExternalSources.CreateFromFile(src_name, tmpfile)
        src.GenerateBlocksFromSource()
        # volitelně udržuj projekt čistý:
        try:
            src.Delete()
        except Exception:
            pass
    finally:
        try:
            os.remove(tmpfile)
        except OSError:
            pass


def _delete_existing_external_source(src_group, name: str):
    """
    Smaže existující ExternalSource se jménem `name`, pokud v projektu už je.
    """
    try:
        # kolekce je iterovatelná; udělej si kopii listu kvůli bezpečnému mazání
        for es in list(src_group.ExternalSources):
            if str(es.Name) == name:
                es.Delete()
    except Exception:
        pass


# --- Příklad použití (volitelné) ---
if __name__ == "__main__":
    #PROJECT = r"C:\Projects\MyTiaProject\MyProject.ap19"   # uprav pro svou verzi/ap*
    PROJECT = r"C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\JAN0837_sample\JAN0837_sample.ap19"
    WITH_UI = True
    TIA_ROOT = r"C:\Program Files\Siemens\Automation\Portal V19"  # nebo None, pokud máš standardní umístění
    VERSION = r"V19"

    tia, proj = open_or_attach_project(PROJECT, WITH_UI, tia_root=TIA_ROOT, version=VERSION)
    
    try:
        plc = get_plc_software(proj, device_name=None)  # nebo např. "PLC_1"
        print("OK, PlcSoftware nalezen:", plc)
        
        # UDT (volitelně)
        create_or_replace_udt(plc, "MyDataType",
                              fields=[("Speed", "Real", "0.0"),
                                      ("Count", "DInt", "0"),
                                      ("Enabled", "Bool", "FALSE")])
        # Jednoduchý DB
        create_or_replace_simple_db(plc, "DB_ProcessData", optimized=True)
        print("Hotovo.")
    finally:
        # velmi důležité kvůli uvolnění licence/procesu
        tia.Dispose()
