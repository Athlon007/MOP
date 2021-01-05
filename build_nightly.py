# MOP Builder
# This script let's you quickly compress new release to .zip file.
# Script version: 1.0.0 (14.12.2019)
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

print("=== Building the release... ===\n")

VERSION = input("MOP version: ")

BASE_DIR = os.getcwd()


def make_zip(files, zipName):
    print('Creating new zip: {0}'.format(zipName))
    NEW_ZIP = ZipFile(BASE_DIR + "\\" + zipName, 'w', zipfile.ZIP_DEFLATED)

    CURSOR_UP_ONE = '\x1b[1A'
    ERASE_LINE = '\x1b[2K'

    x = 1
    for file in files:
        NEW_ZIP.write(file)
        a = x / len(files) * 100
        print("Progress: " + str(int(math.ceil(a))) + "% (" + file + ")")
        sys.stdout.write(CURSOR_UP_ONE)
        sys.stdout.write(ERASE_LINE)
        x += 1

    NEW_ZIP.close()


os.chdir(BASE_DIR)
shutil.rmtree(BASE_DIR + "\\build", True)

os.mkdir("build")
shutil.copyfile("MOP\\bin\\Release\\MOP.dll", "build\\MOP.dll")
os.chdir("build")

FILES = []
FILES.extend(["MOP.dll"])

NOW = str(date.today()).replace("-", "")

make_zip(FILES, "MOP-" + VERSION + "-" + NOW +"-nightly.zip")

print("Done!\nQuitting...")
quit()
