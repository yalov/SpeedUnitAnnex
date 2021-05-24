# Public domain license.
# $version: 1

import sys
from git import Repo

import os.path

from github import Github
from github import GithubException

def PublishToGithub(token, mod_name, version, last_change, is_draft, is_prerelease, zip_file):
    """create tag and publish a release,
    Returns:
        1: succeed
        0: failed
    """
    sys.stdout.write(" * Github connection...")
    github = None
    repo = None
    user = None

    try:
        github = Github(token)
        user = github.get_user()
    except Exception:
        print(" failed.")
        return 0

    try:
        repo = user.get_repo(mod_name)
        print(" user: {}, repo: {}".format(user.login, repo.name))
    except Exception:
        print(" failed to get {}".format(mod_name))
        return 0

    tags = repo.get_tags()
    count = tags.totalCount

    if count == 0 or (count > 0 and tags[0].name != version):
        print(" * getting last commit sha ...")
        sha = repo.get_commits()[0].sha

        try:
            repo.create_git_ref('refs/tags/{}'.format(version), sha)
        except GithubException:
            print("   * could not create tag on the repo.")
    else:
        print(" * the tag "+version+" is found, skiping ...")

    rel = repo.create_git_release(tag=version, name="Version " + version,
                                  message=last_change,
                                  draft=is_draft, prerelease=is_prerelease)

    print(" * uploading asset ...")
    rel.upload_asset(path=zip_file, content_type="application/zip")
    print(" * git fetch origin ...")
    gitrepo = Repo(os.getcwd())
    try:
        gitrepo.remotes.origin.fetch()
    except:
        print(" * fetch() failed, check ssh from the cmd")
        return 0
    else:
        print(" * success.")
        return 1