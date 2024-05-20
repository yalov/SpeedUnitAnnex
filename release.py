#!/usr/bin/python3

"""requirements:
Python3.9
pip install pyperclip GitPython PyGithub
./release_spacedock_utils.py
./release_github.py
make sure, that ssh is set up
    copy private key to ~/.ssh/

Public domain license.
https://github.com/yalov/SpeedUnitAnnex/blob/master/release.py
version: 24

Script loads release-arhive to github and spacedock
you need to set values in the release.json
also you need to place github-token and spacedock login+pass
into a new file: release_token.json (alongside release.json)
{
    "GITHUB_TOKEN": "your_github_token"
    "SPACEDOCK_LOGIN": "your_spacedock_login"
    "SPACEDOCK_PASS": "your_spacedock_password"
}
"""

import sys
def show_exception_and_exit(exc_type, exc_value, tb):
    import traceback
    traceback.print_exception(exc_type, exc_value, tb)
    print("\n" + __doc__ + "\n")
    input("Press key to exit.")
    sys.exit(-1)

sys.excepthook = show_exception_and_exit

import json
import os.path
import re
from shutil import copy, make_archive
import zipfile

from release_github import PublishToGithub
from release_spacedock_utils import GetSpacedockKSPVersions
from release_spacedock_utils import GetSpacedockModDetails
from release_spacedock_utils import PublishToSpacedock





def archive_to(file):
    """Archive GameData and Extras to .zip"""
    if os.path.exists(file):
        os.remove(file)

    if os.path.exists("GameData"):
        make_archive(os.path.splitext(file)[0], 'zip', base_dir="GameData")
    if os.path.exists("Extras"):
        make_archive(os.path.splitext(file)[0], 'zip', base_dir="Extras")

    print("Size: {} byte"
          .format(os.path.getsize(file)))


def get_version(version_file, obj="VERSION", ignore_patch = False):
    """ get version from the version_file """
    data = json.load(open(version_file))
    if obj not in data:
        return "NO"
    ver = data[obj]
    if ignore_patch:
        return "{}.{}".format(ver["MAJOR"], ver["MINOR"])

    version = "{}.{}.{}".format(ver["MAJOR"], ver["MINOR"], ver["PATCH"])

    if "BUILD" in ver and ver["BUILD"] != 0:
        version += '.' + str(ver["BUILD"])
    return version


def get_description(path):
    """ Get description of the last version in the changelog """
    version = r"(#+ )?(Version )?\d(\d)?\.\d(\d)?\.\d(\d)?(\.\d)?(/\d(\d)?\.\d(\d)?\.\d(\d)?(\.\d)?)?( [(\"\')][^\n]*[)\"\')])?"
    changelog = open(path).read()
    pattern = r"\n\s*\n{0}\n(?P<last>.+?)\n({0}|\n\Z|\Z)".format(version)
    desc = re.search(pattern, changelog, re.DOTALL).group('last')
    return desc


if __name__ == '__main__':


    jsn = json.load(open("release.json"))
    tkn = json.load(open("release_token.json"))

    MODNAME = jsn["MODNAME"]
    MODFOLDER = jsn["MODFOLDER"]
    VERSIONFILE = jsn["VERSIONFILE"]
    RELEASESDIR = jsn["RELEASESDIR"]
    CHANGELOG = jsn["CHANGELOG"]
    DRAFT = jsn["DRAFT"]
    PRERELEASE = jsn["PRERELEASE"]
    SD_ID = jsn["SPACEDOCK_ID"]
    FORUM_ID = jsn["FORUM_ID"]


    TOKEN = tkn["GITHUB_TOKEN"]
    SD_LOGIN = tkn["SPACEDOCK_LOGIN"]
    SD_PASS = tkn["SPACEDOCK_PASS"]
    
    is_published = False

    if MODNAME == "auto":
        # parent = os.path.basename([x for x in sys.path if x][0])
        parent = os.path.basename(os.getcwd())
        onlyversionfiles = [f for f in os.listdir() if os.path.splitext(f)[1] == ".version"]
        if (len(onlyversionfiles) == 1 and parent == os.path.splitext(onlyversionfiles[0])[0]):
            MODNAME = parent
        else:
            print("Failed. You need to set up MODNAME in the release.json manually.")
            input("Press Enter to exit")
            sys.exit(-1)

    if MODFOLDER == "auto":
        MODFOLDER = MODNAME

    if VERSIONFILE == "auto":
        VERSIONFILE = MODNAME + ".version"

    copy(VERSIONFILE, "GameData/" + MODFOLDER)
    copy(CHANGELOG, "GameData/" + MODFOLDER)

    VERSION = get_version(VERSIONFILE)
    KSP_VER = get_version(VERSIONFILE, "KSP_VERSION")
    KSP_MIN = get_version(VERSIONFILE, "KSP_VERSION_MIN")
    KSP_MAX = get_version(VERSIONFILE, "KSP_VERSION_MAX")

    print("name: {}".format(MODNAME))
    print("version: {}\nksp_ver: {}\nksp_min: {}\nksp_max: {}\n"
          .format(VERSION, KSP_VER, KSP_MIN, KSP_MAX))
    print("draft: {}\nprerelease: {}\n".format(DRAFT, PRERELEASE))
    print("parsing "+ CHANGELOG +" ...")
    LAST_CHANGE = get_description(CHANGELOG)
    import pyperclip
    pyperclip.copy("Version " + VERSION + "\n" + LAST_CHANGE)
    print("- start of desc ------------")
    print(LAST_CHANGE)
    print("- end of desc --------------")
    print("")


    ZIPFILE = os.path.join(RELEASESDIR, MODNAME + "-v" + VERSION + ".zip")
    if os.path.exists(ZIPFILE):
        print(ZIPFILE + " already exists.")
        if input("Re-zip? [y/N]: ") == 'y':
            archive_to(ZIPFILE)
    else:
        print("Creating "+ ZIPFILE +" ...")
        archive_to(ZIPFILE)


    print("")
    print("GITHUB:")
    print("You already push your changes to a remote repo, don't you?")
    print("Create the tag, and publish a {}{} with the asset?".format("DRAFT " if DRAFT else "","PRERELEASE" if PRERELEASE else "RELEASE" ))
    if input("[y/N]: ") == 'y':
        resp = PublishToGithub(TOKEN, MODNAME, VERSION, LAST_CHANGE, DRAFT, PRERELEASE, ZIPFILE)
        if resp == 1:
            is_published = True

    # ======================================

    print("")
    print("SPACEDOCK:")
    if not SD_ID:
        print("Spacedock number is not found in the json.")
        input("Press Enter to exit")
        sys.exit(-1)

    print("Accessing to Spacedock...")
    try:
        all_versions = [v['name'] for v in GetSpacedockKSPVersions()]
    except:
        print("Failed. Could not access to Spacedock.")
        input("Press Enter to exit")
        sys.exit(-1)

    if KSP_VER not in all_versions:
        print("KSP {} is not supported by Spacedock,\nlast supported version is KSP {}"
              .format(KSP_VER, all_versions[0]))
        print("trying to ignore the patch... ")
        KSP_VER = get_version(VERSIONFILE, "KSP_VERSION", True)
        if KSP_VER not in all_versions:
            print("KSP {} is not supported by Spacedock,\nlast supported version is KSP {}"
                .format(KSP_VER, all_versions[0]))
            print("Release with the last supported by Spacedock {} tag?".format(all_versions[0]))
            if input("[y/N]: ") == 'y':
                KSP_VER = all_versions[0]
            else:
                input("Press Enter to exit")
                sys.exit(-1)

    print("KSP {} is supported by Spacedock.".format(KSP_VER))

    mod_details = GetSpacedockModDetails(SD_ID)

    if not mod_details or 'error' in mod_details:
        print("The mod #{} isn't found.".format(SD_ID))
        input("Press Enter to exit")
        sys.exit(-1)

    print("Spacedock info:\nID: {}, NAME: {}\nLast Release {} (KSP {})".format(
        SD_ID, mod_details['name'],
        mod_details['versions'][0]['friendly_version'],
        mod_details['versions'][0]['game_version']
        ))

    print("Publish {} (KSP {}) to the Spacedock?".format(VERSION, KSP_VER))
    if input("[y/N]: ") == 'y':
        resp = PublishToSpacedock(SD_ID, ZIPFILE, LAST_CHANGE, VERSION, KSP_VER, SD_LOGIN, SD_PASS)
        if not 'error' in resp:
            is_published = True

    if FORUM_ID and is_published:
        import webbrowser
        webbrowser.open(f"https://forum.kerbalspaceprogram.com/index.php?/topic/{FORUM_ID}-*", new=2, autoraise=False)
        print("The forum page is opened.")

    input("Press Enter to exit")
    sys.exit(0)
