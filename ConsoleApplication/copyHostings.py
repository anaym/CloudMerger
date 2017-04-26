from os.path import abspath, dirname, join, exists, isdir
from os import walk, mkdir
from shutil import copyfile
import glob

ignored = {"Test", "Core", "obj", "GuiPrimitives"}

debug = dirname(abspath(__file__))
bin = dirname(debug)
console_application = dirname(bin)
cloud_merger = dirname(console_application)
hostings = join(cloud_merger, "hostings")

def is_ignored_dll(root, file):
    for i in ignored:
        if i in root or i in file:
            return True
    return False

dlls = set()
print("HOSTINGS DETECTED:")
for root, dirs, files in walk(hostings):
    for file in files:
        if file.endswith(".dll"):
            hname = file.strip(".dll")
            # print(hname, root)
            if not is_ignored_dll(root, file):
                    print("+--: " + join(root, file))
                    dlls.add((root, file))

dest = join(debug, "hostings")
if not exists(dest):
    mkdir(dest)

if not isdir(dest):
    raise TypeError("Expected directory")

for root, dll in dlls:
    copyfile(join(root, dll), join(dest, dll))
