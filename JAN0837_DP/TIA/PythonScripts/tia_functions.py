import os
import sys
import tempfile
from pathlib import Path

import tia_parameters as params
import importTIADLL

def import_tia_dll(dll_dir: str) -> int:
	"""Load Siemens.Engineering from the given PublicAPI folder using importTIADLL.

	Returns the importTIADLL return code (0 on success). Does not raise.
	"""
	try:
		sys.argv = ["importTIADLL.py", "--dir", dll_dir]
		rc = importTIADLL.main()
		return int(rc) if isinstance(rc, int) else 0
	except Exception:
		return 1

def open_tia_portal(use_ui: bool):
	"""Open TIA Portal in UI or background mode. Returns (portal, mode)."""
	from Siemens.Engineering import TiaPortal, TiaPortalMode

	mode = TiaPortalMode.WithUserInterface if use_ui else TiaPortalMode.WithoutUserInterface
	try:
		portal = TiaPortal(mode)
		return portal, mode
	except Exception:
		# Fallback: try UI mode if background fails
		try:
			portal = TiaPortal(TiaPortalMode.WithUserInterface)
			return portal, TiaPortalMode.WithUserInterface
		except Exception:
			raise

def locate_project_file(path_like: str) -> Path:
	"""Resolve a project path (folder or .apXX file) to the actual .ap file path.

	If a directory is provided, tries <dir>/<dir_name>.ap* then first *.ap* (any version).
	Raises FileNotFoundError if none found.
	"""
	p = Path(path_like)
	if p.is_file():
		return p

	if not p.exists() or not p.is_dir():
		raise FileNotFoundError(f"Project path does not exist: {p}")

	folder_name = p.name
	# Try to find project file matching folder name with any .ap* extension
	ap_candidates = list(p.glob(f"{folder_name}.ap*"))
	if ap_candidates:
		return ap_candidates[0]

	# Fall back to any .ap* file in the directory
	ap_files = list(p.glob("*.ap*"))
	if ap_files:
		return ap_files[0]

	raise FileNotFoundError(f"No .ap* file found in directory: {p}")


def open_project(portal, project_file: Path):
	"""Open a TIA project by file path. Returns the Project instance."""
	from System.IO import FileInfo
	return portal.Projects.Open(FileInfo(str(project_file)))


def get_service_generic(item, service_type_name: str):
	"""Get service using generic method invocation with GetService<T>()."""
	from System.Reflection import Assembly

	try:
		asm = Assembly.Load("Siemens.Engineering")
		service_type = None
		for t in asm.GetTypes():
			if t.Name == service_type_name:
				service_type = t
				break
		if service_type:
			m = item.GetType().GetMethod("GetService")
			if m and m.IsGenericMethodDefinition:
				gm = m.MakeGenericMethod(service_type)
				return gm.Invoke(item, None)
	except Exception:
		pass
	return None


def find_plc_software(dev):
	"""Find PLC Software via SoftwareContainer using generic GetService<T>().

	Returns (item_with_service, plc_software) or (None, None).
	"""
	from System.Reflection import Assembly

	try:
		asm = Assembly.Load("Siemens.Engineering")
		software_container_type = None
		for t in asm.GetTypes():
			if t.Name == "SoftwareContainer":
				software_container_type = t
				break

		if not software_container_type:
			return None, None

		# try device
		try:
			m = dev.GetType().GetMethod("GetService")
			if m and m.IsGenericMethodDefinition:
				gm = m.MakeGenericMethod(software_container_type)
				r = gm.Invoke(dev, None)
				if r and hasattr(r, "Software"):
					return dev, r.Software
		except Exception:
			pass

		# try direct children
		try:
			for it in dev.DeviceItems:
				m = it.GetType().GetMethod("GetService")
				if m and m.IsGenericMethodDefinition:
					gm = m.MakeGenericMethod(software_container_type)
					r = gm.Invoke(it, None)
					if r and hasattr(r, "Software"):
						return it, r.Software
				# nested children
				try:
					for ch in it.DeviceItems:
						m = ch.GetType().GetMethod("GetService")
						if m and m.IsGenericMethodDefinition:
							gm = m.MakeGenericMethod(software_container_type)
							r = gm.Invoke(ch, None)
							if r and hasattr(r, "Software"):
								return ch, r.Software
				except Exception:
					pass
		except Exception:
			pass
	except Exception:
		pass
	return None, None


def set_enum_prop(obj, prop_name: str, value_name: str) -> bool:
	"""Set an enum property by member name. Returns True on success."""
	from System import Enum
	if obj is None:
		return False
	p = obj.GetType().GetProperty(prop_name)
	if p is None or not p.PropertyType.IsEnum:
		return False
	v = Enum.Parse(p.PropertyType, value_name)
	p.SetValue(obj, v, None)
	return True


def set_bool_prop(obj, prop_name: str, value: bool) -> bool:
	"""Set a boolean property. Returns True on success."""
	if obj is None:
		return False
	p = obj.GetType().GetProperty(prop_name)
	if p is None or str(p.PropertyType.FullName) != "System.Boolean":
		return False
	p.SetValue(obj, value, None)
	return True


def _delete_existing_external_source(src_group, name: str):
	"""Delete existing ExternalSource with given name if it exists."""
	try:
		for es in list(src_group.ExternalSources):
			if str(es.Name) == name:
				es.Delete()
	except Exception:
		pass


def create_or_replace_udt(plc, type_name: str, fields: list | None = None) -> None:
	"""Create/replace a PLC data type (UDT) via TYPE/END_TYPE SCL external source."""
	from Siemens.Engineering.SW.Blocks import PlcBlock

	fields = fields or params.DEFAULT_UDT_FIELDS

	lines = []
	for (name, typ, init) in fields:
		t = str(typ).upper()
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

	# Delete existing UDT with same name
	for b in list(plc.BlockGroup.Blocks):
		if isinstance(b, PlcBlock) and str(b.Name) == type_name:
			b.Delete()
			break

	src_group = plc.ExternalSourceGroup
	src_name = f"TYPE_{type_name}.scl"

	_delete_existing_external_source(src_group, src_name)

	tmpfile = os.path.join(tempfile.gettempdir(), src_name)
	with open(tmpfile, "w", encoding="utf-8") as f:
		f.write(type_scl)

	try:
		src = src_group.ExternalSources.CreateFromFile(src_name, tmpfile)
		src.GenerateBlocksFromSource()
		try:
			src.Delete()
		except Exception:
			pass
	finally:
		try:
			os.remove(tmpfile)
		except OSError:
			pass


def create_or_replace_simple_db(plc, db_name: str, udt_type: str | None = None, optimized: bool = True) -> None:
	"""Create/replace a simple DB via SCL external source."""
	from Siemens.Engineering.SW.Blocks import PlcBlock

	# Delete existing DB with same name
	for b in list(plc.BlockGroup.Blocks):
		if isinstance(b, PlcBlock) and str(b.Name) == db_name:
			b.Delete()
			break

	attr = "{ S7_Optimized_Access := 'TRUE' }" if optimized else "{ S7_Optimized_Access := 'FALSE' }"

	if udt_type:
		db_scl = f"""DATA_BLOCK {db_name}
{attr}
VERSION : 0.1
  VAR
	data : {udt_type};
  END_VAR
BEGIN
END_DATA_BLOCK
"""
	else:
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

	src_group = plc.ExternalSourceGroup

	file_stub = db_name
	if file_stub.upper().startswith("DB_"):
		file_stub = file_stub[3:]
	src_name = f"DB_{file_stub}.scl"

	_delete_existing_external_source(src_group, src_name)

	tmpfile = os.path.join(tempfile.gettempdir(), src_name)
	with open(tmpfile, "w", encoding="utf-8") as f:
		f.write(db_scl)

	try:
		src = src_group.ExternalSources.CreateFromFile(src_name, tmpfile)
		src.GenerateBlocksFromSource()
		try:
			src.Delete()
		except Exception:
			pass
	finally:
		try:
			os.remove(tmpfile)
		except OSError:
			pass


def create_or_replace_db_with_multiple_udts(plc, db_name: str, udt_vars: list[tuple[str, str]], optimized: bool = True) -> None:
	"""Create/replace a DB with multiple UDT-typed variables.
	
	Args:
		plc: PLC software instance
		db_name: Name of the DB to create
		udt_vars: List of (variable_name, udt_type) tuples, e.g. [("input", "Input_UDT"), ("output", "Output_UDT")]
		optimized: Whether to use optimized access
	"""
	from Siemens.Engineering.SW.Blocks import PlcBlock

	# Delete existing DB with same name
	for b in list(plc.BlockGroup.Blocks):
		if isinstance(b, PlcBlock) and str(b.Name) == db_name:
			b.Delete()
			break

	attr = "{ S7_Optimized_Access := 'TRUE' }" if optimized else "{ S7_Optimized_Access := 'FALSE' }"

	# Build VAR section with multiple UDT variables
	var_lines = []
	for var_name, udt_type in udt_vars:
		var_lines.append(f"    {var_name} : {udt_type};")
	var_section = "\n".join(var_lines)

	db_scl = f"""DATA_BLOCK {db_name}
{attr}
VERSION : 0.1
  VAR
{var_section}
  END_VAR
BEGIN
END_DATA_BLOCK
"""

	src_group = plc.ExternalSourceGroup

	file_stub = db_name
	if file_stub.upper().startswith("DB_"):
		file_stub = file_stub[3:]
	src_name = f"DB_{file_stub}.scl"

	_delete_existing_external_source(src_group, src_name)

	tmpfile = os.path.join(tempfile.gettempdir(), src_name)
	with open(tmpfile, "w", encoding="utf-8") as f:
		f.write(db_scl)

	try:
		src = src_group.ExternalSources.CreateFromFile(src_name, tmpfile)
		src.GenerateBlocksFromSource()
		try:
			src.Delete()
		except Exception:
			pass
	finally:
		try:
			os.remove(tmpfile)
		except OSError:
			pass


def save_project(project):
	"""Save the project, ignoring minor exceptions."""
	try:
		project.Save()
	except Exception:
		pass


def close_if_headless(project, portal, mode):
	"""Close project and portal if running without UI."""
	try:
		from Siemens.Engineering import TiaPortalMode
		if mode == TiaPortalMode.WithoutUserInterface:
			if project is not None:
				try:
					project.Close()
				except Exception:
					pass
			if portal is not None:
				try:
					portal.Dispose()
				except Exception:
					pass
	except Exception:
		pass






