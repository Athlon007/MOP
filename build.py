# MOP Builder
# This script let's you quickly compress new release to .zip file.
# Script version: 2.0 (25.01.2021)
#
# This file is distributed under the same license as the MOP is.

import os
import sys
import zipfile
from zipfile import ZipFile
from array import array
import shutil
import math
from datetime import date
from os import path

print("=== Building the release... ===\n")

MOD_NAME = "MOP"
BASE_DIR = os.getcwd()


def make_zip(files, zipName):
    print('Creating new zip: {0}'.format(zipName))
    NEW_ZIP = ZipFile("{}/{}".format(BASE_DIR, zipName),
                      'w', zipfile.ZIP_DEFLATED)

    CURSOR_UP_ONE = '\x1b[1A'
    ERASE_LINE = '\x1b[2K'

    x = 1
    for file in files:
        NEW_ZIP.write(file)
        a = x / len(files) * 100
        print("Progress: {}% ({})".format(str(int(math.ceil(a))), file))
        sys.stdout.write(CURSOR_UP_ONE)
        sys.stdout.write(ERASE_LINE)
        x += 1

    NEW_ZIP.close()


os.chdir(BASE_DIR)
shutil.rmtree("{}/build".format(BASE_DIR), True)

# Check if binary exists.
if not path.exists("{}/bin/Release/{}.dll".format(MOD_NAME, MOD_NAME)):
    print("{}.dll does not exist!".format(MOD_NAME))
    quit()

os.mkdir("build")
shutil.copyfile("{}/bin/Release/{}.dll".format(MOD_NAME,
                                               MOD_NAME), "build/{}.dll".format(MOD_NAME))
os.chdir("build")

FILES = []
FILES.extend(["{}.dll".format(MOD_NAME)])
ZIP_NAME = "{}.zip".format(MOD_NAME)

option = input('Choose option:\n1 - Make release \n2 - Make nightly\nQ - Quit')

if option == 2:
    NOW = str(date.today()).replace("-", "")
    VERSION = input("{} version: ".format(MOD_NAME))
    ZIP_NAME = "{}-{}-{}-nightly.zip".format(MOD_NAME, VERSION, NOW)

elif option.lower() == 'q':
    quit()

make_zip(FILES, "build/{}".format(ZIP_NAME))

print("Done!\nQuitting...")
quit()
