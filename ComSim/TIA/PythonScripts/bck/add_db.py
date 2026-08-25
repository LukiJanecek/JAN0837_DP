# this code works properly

# --- helpery (vložit jednou, pod import blok a před použití) ---

from System.Reflection import Assembly, ReflectionTypeLoadException

asm = Assembly.Load("Siemens.Engineering")
def _all_types(a):
    try:
        return list(a.GetTypes())
    except ReflectionTypeLoadException as e:
        return [t for t in e.Types if t is not None]
_TYPES = _all_types(asm)

def find_enum(simple_name: str):
    for t in _TYPES:
        if t is not None and t.IsEnum and t.Name == simple_name:
            return t
    return None

def enum_val(enum_type, name: str):
    return Enum.Parse(enum_type, name)

def find_cpu_item(root_item):
    # projde strom a najde item, na kterém je SoftwareContainer
    queue = [root_item]
    while queue:
        it = queue.pop(0)
        try:
            if it.GetService("SoftwareContainer") is not None:
                return it
        except:
            pass
        try:
            for ch in it.DeviceItems:
                queue.append(ch)
        except:
            pass
    return None

def set_enum_prop(obj, prop_name, enum_name, value_name):
    if obj is None: return False
    p = obj.GetType().GetProperty(prop_name)
    if p is None: 
        print(f"[WARN] Vlastnost {prop_name} nenalezena.")
        return False
    et = p.PropertyType
    if not et.IsEnum:
        print(f"[WARN] {prop_name} není enum.")
        return False
    try:
        v = Enum.Parse(et, value_name)
        p.SetValue(obj, v, None)
        print(f"[OK] {prop_name} = {value_name} ({et.FullName})")
        return True
    except Exception as ex:
        print(f"[WARN] {et.FullName} nemá hodnotu '{value_name}': {ex}")
        return False

def set_bool_prop(obj, prop_name, value: bool):
    if obj is None: return False
    p = obj.GetType().GetProperty(prop_name)
    if p is None or str(p.PropertyType.FullName) != "System.Boolean":
        return False
    p.SetValue(obj, value, None)
    print(f"[OK] {prop_name} = {value}")
    return True

def create_var_with_datatype(static_iface, var_name: str, datatype_name: str) -> bool:
    # najde overload Static.Create(string, <enum DataType>) a zavolá jej
    t = static_iface.GetType()
    for m in t.GetMethods():
        if m.Name != "Create": 
            continue
        pars = m.GetParameters()
        if len(pars) != 2: 
            continue
        if str(pars[0].ParameterType.FullName) != "System.String":
            continue
        dt = pars[1].ParameterType
        if dt.IsEnum and dt.Name == "DataType":
            try:
                v = Enum.Parse(dt, datatype_name)
                static_iface.Create(var_name, v)
                print(f"[OK] Vytvořeno: {var_name} : DataType.{datatype_name}")
                return True
            except Exception as ex:
                print(f"[WARN] {dt.FullName} nemá hodnotu '{datatype_name}': {ex}")
                return False
    print("[WARN] Nenašel jsem Create(string, <enum DataType>).")
    return False
# --- /helpery ---

# Parametry PLC (můžeš dát z C#)
cpu_type_id = "CPU_1212C_DC_DC_DC"
cpu_version = "V4.5"
cpu_name = "PLC_1"

import clr as _clr
deviceItemRef = _clr.Reference[object]()
device = project.Devices.CreateWithItem(cpu_type_id, cpu_version, cpu_name, deviceItemRef)
root_item = deviceItemRef.Value
print("[OK] PLC zařízení přidáno.")

cpu_item = find_cpu_item(root_item)
if cpu_item is None:
    raise RuntimeError("CPU item se SoftwareContainer nebyl nalezen.")

# Protection
prot = cpu_item.GetService("Protection")
if prot:
    set_enum_prop(prot, "Level", "ProtectionLevel", "FullAccess")
    set_bool_prop(prot, "DownloadWithoutRewire", True)
else:
    print("[WARN] Protection service není k dispozici.")

# StartInfo
start_info = cpu_item.GetService("StartInfo")
if start_info:
    set_enum_prop(start_info, "StartMode", "StartMode", "AlwaysRun")
else:
    print("[WARN] StartInfo service není k dispozici.")

# PLC Software + DB
swc = cpu_item.GetService("SoftwareContainer")
if swc is None:
    raise RuntimeError("SoftwareContainer na CPU itemu nenalezen.")
plc_sw = swc.Software

# najdi enumy pro BlockType/Language (když nepůjdou importy)
PlcBlockType = find_enum("PlcBlockType")
PlcProgrammingLanguage = find_enum("PlcProgrammingLanguage")
if PlcBlockType is None or PlcProgrammingLanguage is None:
    raise RuntimeError("Nepodařilo se najít PlcBlockType/PlcProgrammingLanguage.")

db = plc_sw.BlockGroup.Blocks.Create(
    enum_val(PlcBlockType, "DataBlock"), 
    "MyDataBlock", 
    enum_val(PlcProgrammingLanguage, "LAD")
)
create_var_with_datatype(db.Interface.Static, "myRealVar", "Real")

project.Save()
print("[OK] Projekt uložen.")
