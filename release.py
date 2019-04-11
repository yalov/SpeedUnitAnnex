"""requirements: 
Python3, pip install PyGithub, release_spacedock_utils.py

Public domain license.
author: flart, version: 9
https://github.com/yalov/SpeedUnitAnnex/blob/master/release.py

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
import json
import os.path
import re
from shutil import copy
import zipfile

from github import Github
from github import GithubException

from release_spacedock_utils import GetSpacedockKSPVersions
from release_spacedock_utils import GetSpacedockModDetails
from release_spacedock_utils import PublishToSpacedock


def zipdir(path, ziph):
    """ recursive archiving path folder to ziph (zipfile handle) """
    for root, _, files in os.walk(path):
        for file in files:
            ziph.write(os.path.join(root, file))


def archive_to(file):
    """Archive GameData and Extras to .zip"""
    if os.path.exists(file):
        os.remove(file)

    zipf = zipfile.ZipFile(file, 'x', zipfile.ZIP_DEFLATED)
    if os.path.exists("GameData"):
        zipdir("GameData", zipf)
    if os.path.exists("Extras"):
        zipdir("Extras", zipf)
    zipf.close()
    print("{}, files: {}, size: {} byte"
          .format("valid ZIP" if zipfile.is_zipfile(file) else "FAIL",
                  len(zipf.infolist()), os.path.getsize(file)))


def get_version(version_file, obj="VERSION"):
    """ get version from the version_file """
    data = json.load(open(version_file))
    if obj not in data:
        return "NO"
    ver = data[obj]
    version = "{}.{}.{}".format(ver["MAJOR"], ver["MINOR"], ver["PATCH"])
    if "BUILD" in ver and ver["BUILD"] != 0:
        version += '.' + str(ver["BUILD"])
    return version


def get_description(path):
    """ Get description of the last version in the changelog """
    version = r"(#+ )?(Version )?\d\.\d\.\d(\.\d)?(/\d\.\d\.\d(\.\d)?)?( [(\"\')][^\n]*[)\"\')])?"
    changelog = open(path).read()
    pattern = r"\n\s*\n{0}\n(?P<last>.+?)\n({0}|\n\Z|\Z)".format(version)
    desc = re.search(pattern, changelog, re.DOTALL).group('last')
    return desc


def publish_to_github(token, mod_name, version, last_change, is_draft, is_prerelease, zip_file):
    """create tag and publish a release"""
    sys.stdout.write(" * Github connection...")
    github = None
    repo = None
    user = None

    try:
        github = Github(token)
        user = github.get_user()
    except Exception:
        print(" failed.")
        sys.exit(-1)

    try:
        repo = user.get_repo(mod_name)
        print(" user: {}, repo: {}".format(user.login, repo.name))
    except Exception:
        print(" failed to get {}".format(mod_name))
        sys.exit(-1)

    tags = repo.get_tags()
    count = tags.totalCount

    if count == 0 or (count > 0 and tags[0].name != version):
        print(" * getting last commit sha...")
        sha = repo.get_commits()[0].sha

        try:
            repo.create_git_ref('refs/tags/{}'.format(version), sha)
        except GithubException:
            print("   * could not create tag on the repo.")
    else:
        print(" * the tag "+version+" is found, skiping...")

    rel = repo.create_git_release(tag=version, name="Version " + version,
                                  message=last_change,
                                  draft=is_draft, prerelease=is_prerelease)

    print(" * uploading asset...")
    rel.upload_asset(path=zip_file, content_type="application/zip")
    print(" * success.")


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

    TOKEN = tkn["GITHUB_TOKEN"]
    SD_LOGIN = tkn["SPACEDOCK_LOGIN"]
    SD_PASS = tkn["SPACEDOCK_PASS"]

    if MODNAME == "auto":
        parent = os.path.basename([x for x in sys.path if x][0])
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
        publish_to_github(TOKEN, MODNAME, VERSION, LAST_CHANGE, DRAFT, PRERELEASE, ZIPFILE)

    # ======================================
    
    print("")    
    print("SPACEDOCK:")
    if not SD_ID:
        print("Spacedock number is not found in the json.")
        input("Press Enter to exit")
        sys.exit(-1)

    print("Accessing to Spacedock...")
    all_versions = [v['name'] for v in GetSpacedockKSPVersions()]

    if not all_versions:
        print("Failed. Could not access to Spacedock.")
        input("Press Enter to exit")
        sys.exit(-1)

    if KSP_VER not in all_versions:
        print("KSP {} is not supported by Spacedock,\nlast supported version is KSP {}"
              .format(KSP_VER, all_versions[0]))
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
        PublishToSpacedock(SD_ID, ZIPFILE, LAST_CHANGE, VERSION, KSP_VER, SD_LOGIN, SD_PASS)

    input("Press Enter to exit")
    sys.exit(0)
