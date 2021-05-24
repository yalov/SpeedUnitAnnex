"""utils for accessing to the spacedock"""
# Public domain license.
# Based on: https://github.com/ihsoft/KSPDev_ReleaseBuilder
# $version: 3

import json
import urllib.request
import urllib.error
import urllib.parse

import io
import random
import string
import ntpath


# Endpoint for all the API requests
API_BASE_URL = 'https://spacedock.info'

# The actions paths.
API_AUTHORIZE = '/api/login'
API_UPDATE_MOD_TMPL = '/api/mod/{mod_id}/update'
# API_GET_VERSIONS = '/api/kspversions'
API_GET_VERSIONS = '/api/3102/versions'
API_GET_MOD = '/api/mod/{mod_id}'

# The authorization cookie. It's only created once. To refresh it, simply
#  set it to None.
authorized_cookie = None


def PublishToSpacedock(mod_id, filepath, changelog, mod_version, game_version, login, password):
    """
    Args:
      mod_id: The mod ID to update.
      filepath: A full or relative path to the local file.
      changelog: The change log content.
      mod_version: The version of the mod being published.
      game_version: The KSP version to publish for.
    Returns:
      The response object.
    """

    headers, data = EncodeFormData([
        {'name': 'version', 'data': mod_version},
        {'name': 'changelog', 'data': changelog},
        {'name': 'game-version', 'data': game_version},
        {'name': 'notify-followers', 'data': 'yes'},
        {'name': 'zipball', 'filename': filepath},
    ])
    url, headers = _GetAuthorizedEndpoint(
        API_UPDATE_MOD_TMPL, headers, login, password, mod_id=mod_id)

    resp = _CallAPI(url, data=data, headers=headers)

    print('success.')
    print('The new version is now available and the followers are notified!')

    return resp


def GetSpacedockModDetails(mod_id):
    """Gets the mod informnation.
    This call does NOT require authorization.
    Args:
      mod_id: The mod to request.
    Returns:
      The response object.
    """
    url = _MakeAPIUrl(API_GET_MOD, mod_id=mod_id)
    response_obj, _ = _CallAPI(url, None, None)
    return response_obj


def GetSpacedockKSPVersions():
    """Gets the available versions of the game.
    This call does NOT require authorization.
    Returns:
      A list of objects: { 'name': <KSP name>, 'id': <Spacedock ID> }
    """
    response = _CallAPI(_MakeAPIUrl(API_GET_VERSIONS), None, None)
    versions = [{'name': x['friendly_version'], 'id': x['id']} for x in response[0]]

    return versions


def _MakeAPIUrl(action_path, **kwargs):
    """Makes a URL for the action."""
    return API_BASE_URL + action_path.format(**kwargs)


def _CallAPI(url, data, headers, raise_on_error=True):
    """Invokes the API call."""
    resp_obj = {'error': True, 'reason': 'unknown'}
    try:
        request = urllib.request.Request(url, data, headers=headers or {})
        response = urllib.request.urlopen(request)
        resp_obj = json.loads(response.read())
        headers = response.info()
    except urllib.error.HTTPError as ex:
        resp_obj = {'error': True, 'reason': '%d - %s' % (ex.code, ex.reason)}
        try:
            resp_obj = json.loads(ex.read())
        except Exception:
            pass  # Not a JSON response
        if ex.code == 401:
            print("AuthorizationRequiredError")

    if type(resp_obj) is dict and resp_obj.get('error'):
        if raise_on_error:
            print("BadResponseError")
        return resp_obj, None
    return resp_obj, headers


def _GetAuthorizedEndpoint(api_path, headers, login, password, **kwargs):
    """Gets API URL and the authorization headers.

    The login/password must be set in the global variables API_LOGIN/API_PASS.
    """
    global authorized_cookie

    url = _MakeAPIUrl(api_path, **kwargs)
    if not headers:
        headers = {}

    if not authorized_cookie:
        if not login or not password:
            print("BadCredentialsError: API_LOGIN and/or API_PASS not set")
            exit(0)

        auth_headers, data = EncodeFormData([
            {'name': 'username', 'data': login},
            {'name': 'password', 'data': password},
        ])
        resp, auth_headers = _CallAPI(
            API_BASE_URL + API_AUTHORIZE, data, auth_headers,
            raise_on_error=False)
        if resp['error']:
            print("BadCredentialsError")
            exit(0)
        authorized_cookie = auth_headers['set-cookie']

    headers['Cookie'] = authorized_cookie
    return url, headers


# Utils: Provides helpers to deal with the multipart/form-data MIME types.


def EncodeFormData(fields):
    """Encodes the provided arguments as the web form fields.

    The argument must be an array of objects that define the fields to be passed:
    - 'name': A required property than defines the name of the web form field.
    - 'data': The raw data to pass. If it's of type 'string', then it's passed as
      'text/plain'. Otherwise, it's serialized as JSON and passed as
      'application/json'.
    - 'filename': An optional property that designates a path to the binary file
      to tansfer. The 'data' field is ignored in this case, and the real data is
      read from the file. The data is passed as 'application/octet-stream', and
      server will receive the file name part of the path. The part can be
      relative, in which case it's counted realtive to the main module, or it can
      be absolute.

    Example:
      [
          { 'name': 'metadata', 'data': metadatObj },
          { 'name': 'file', 'filename': '/home/me/releases/MyMod_v1.0.zip' }
      ]

    @param fields: An array of the fields to encode.
    """
    boundry = '----WebKitFormBoundary' + IdGenerator(16)

    data_stream = io.BytesIO()
    for field in fields:
        filename = field.get('filename')
        if filename:
            with open(filename, 'rb') as f:
                data = f.read()
            content_type = 'application/octet-stream'
            filename = ntpath.basename(filename)
        else:
            data = field['data']
            if type(data) != str:
                content_type = 'application/json'
                data = json.dumps(data)
            else:
                content_type = 'text/plain; charset=utf-8'
                data = data.encode('utf-8')
        _WriteFormData(data_stream, boundry,
                       field['name'], content_type, data, filename)

    data_stream.write(b'--')
    data_stream.write(boundry.encode())
    data_stream.write(b'--')
    data_stream.write(b'\r\n')

    headers = {
        'Content-Type': 'multipart/form-data; boundary=%s' % boundry,
        'Content-Length': str(data_stream.tell()),
    }
    return headers, data_stream.getvalue()


def IdGenerator(size, chars=string.ascii_letters + string.digits):
    """Makes a unique random string of the requested size."""
    return ''.join(random.choice(chars) for _ in range(size))


def _WriteFormData(stream, boundry, name, content_type, data, filename=None):
    """Helper method to write a single web form field."""
    stream.write(b'--')
    stream.write(boundry.encode())
    stream.write(b'\r\n')

    if filename:
        stream.write(('Content-Disposition: form-data; name="%s"; filename="%s"' %
                      (name, filename)).encode())
    else:
        stream.write(('Content-Disposition: form-data; name="%s"' %
                      (name)).encode())
    stream.write(b'\r\n')
    stream.write(('Content-Type: %s' % content_type).encode())
    stream.write(b'\r\n')
    stream.write(b'\r\n')
    stream.write(data)
    stream.write(b'\r\n')
