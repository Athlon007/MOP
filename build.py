# MOP Builder
# This script let's you quickly compress new release to .zip file.
# Script version: 2.1 (26.01.2021)
#
# This file is distributed under the same license as the MOP is.

import os
import sys
import zipfile
from zipfile import ZipFile
import shutil
import math
from datetime import date
from os import path

print("=== Building the release... ===\n")

# CONFIG #
# Name of the build folder.
MOD_NAME = "MOP"
# List of files that we want to pack up.
FILES_TO_PACK = ["/bin/Release/MOP.dll"]

BASE_DIR = os.getcwd()


def make_zip(files, zipName):
    print('Creating new zip: {0}'.format(zipName))
    new_zip = ZipFile("{}/{}".format(BASE_DIR, zipName),
                      'w', zipfile.ZIP_DEFLATED)

    CURSOR_UP_ONE = '\x1b[1A'
    ERASE_LINE = '\x1b[2K'

    x = 1
    for file in files:
        new_zip.write(file)
        a = x / len(files) * 100
        print("Progress: {}% ({})".format(str(int(math.ceil(a))), file))
        sys.stdout.write(CURSOR_UP_ONE)
        sys.stdout.write(ERASE_LINE)
        x += 1

    new_zip.close()


def get_file_name_from_path(path):
    name = path.split("/")
    return name[len(name) - 1]


os.chdir(BASE_DIR)
shutil.rmtree("{}/build".format(BASE_DIR), True)

# Check if binary exists.
if not path.exists("{}/bin/Release/{}.dll".format(MOD_NAME, MOD_NAME)):
    print("{}.dll does not exist!".format(MOD_NAME))
    quit()

os.mkdir("build")

files = []

for file in FILES_TO_PACK:
    file_name = get_file_name_from_path(file)
    shutil.copyfile("{}/{}".format(MOD_NAME, file),
                    "build/{}".format(file_name))

os.chdir("build")

for file in FILES_TO_PACK:
    file_name = get_file_name_from_path(file)
    files.extend([file_name])

zip_name = "{}.zip".format(MOD_NAME)

option = input(
    'Choose option:\n1 - Make release \n2 - Make nightly\nQ - Quit\n\n')
print("\n")

if option == '2':
    now = str(date.today()).replace("-", "")

    # Read version from MOP.cs
    mainfile = ""
    with open("{}/{}/src/{}.cs".format(BASE_DIR, MOD_NAME, MOD_NAME), "r") as file:
        mainfile = file.readlines()

    version_string = ""
    for line in mainfile:
        if line.__contains__("override string Version"):
            version_string = line

    version_string = version_string.split("\"")[1]

    zip_name = "{}-{}-{}-nightly.zip".format(MOD_NAME, version_string, now)

elif option.lower() == 'q':
    quit()

make_zip(files, "build/{}".format(zip_name))

print("Done!\nQuitting...")
quit()
