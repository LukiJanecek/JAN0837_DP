# import Siemens.Engineering 
# params:
#   path 
#   TIA path

# create blank project
# params:
#   path
#   name
#   cpu_type_id
#   cpu_version
#   cpu_name

# open project on path
# params:
#   path
#   name
#   cpu_name

# create DB and add variables
# params:
#   project path
#   cpu_name
#   db_name
#   variables
#       name, datatype, value 
# create datatype in PLC and then use it in DB

# start TIA Portal with ui 

# výběr aktuálních plc do selectboxu
# připojit mou knihovnu do projektu? 
# PLC vytvořit datatype a z něj pak čerpat do DB


if __name__ == "__main__":
    try:
        # Do this
        print("Doing stuff...")
    except Exception as e:
        print(f"Error: {e}") 
    finally:
        print("It is done. I have spoken.")


