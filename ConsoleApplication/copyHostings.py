from os.path import abspath, dirname, join, exists, isdir
from os import walk, mkdir
from shutil import copyfile
import glob


# print("Builded!")
# print(__file__)

debug = dirname(abspath(__file__))
bin = dirname(debug)
console_application = dirname(bin)
cloud_merger = dirname(console_application)
hostings = join(cloud_merger, "hostings")

dlls = []
print("HOSTINGS DETECTED:")
for root, dirs, files in walk(hostings):
    for file in files:
        if file.endswith(".dll"):
            hname = file.strip(".dll")
            # print(hname, root)
            if ("\\" + hname + "\\") in root or ("/" + hname + "/") in root:
                if "Test" not in root and "obj" not in root:
                    print("+--: " + join(root, file))
                    dlls.append((root, file))

dest = join(debug, "hostings")
if not exists(dest):
    mkdir(dest)

if not isdir(dest):
    raise TypeError("Expected directory")

for root, dll in dlls:
    copyfile(join(root, dll), join(dest, dll))

# print(cloud_merger)
