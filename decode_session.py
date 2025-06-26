import base64
import gzip
import json
from typing import Any, Dict, Union

TOOL_MAP = {
    0: "Pencil",
    1: "Ink",
    2: "Airbrush",
    3: "Eraser",
}

SHAPE_MAP = {
    "cb": "cube",
    "sp": "sphere",
}

def decode_session(payload: Union[str, Dict[str, Any]]) -> Dict[str, Any]:
    """Decode compressed drawing session data.

    Parameters
    ----------
    payload : Union[str, dict]
        Either a dictionary with short keys or a base64 string of gzipped JSON.

    Returns
    -------
    dict
        A dictionary with expanded keys and human-readable values.
    """
    if isinstance(payload, str):
        # base64 decode then gzip decompress
        data = base64.b64decode(payload)
        data = gzip.decompress(data)
        payload = json.loads(data.decode("utf-8"))
    elif not isinstance(payload, dict):
        raise TypeError("payload must be a dict or base64 encoded string")

    decoded: Dict[str, Any] = {}
    for key, value in payload.items():
        if key == "p":
            decoded["promptID"] = value
        elif key == "s":
            decoded["symmetryEnabled"] = value
        elif key == "sh":
            if isinstance(value, list):
                decoded["shapeAssetsUsed"] = [SHAPE_MAP.get(v, v) for v in value]
            else:
                decoded["shapeAssetsUsed"] = SHAPE_MAP.get(value, value)
        elif key == "t":
            decoded_list = []
            if isinstance(value, list):
                for item in value:
                    if isinstance(item, list) and len(item) == 2:
                        timestamp, tool = item
                        decoded_list.append([timestamp, TOOL_MAP.get(tool, tool)])
                    else:
                        decoded_list.append(TOOL_MAP.get(item, item))
            else:
                decoded_list.append(TOOL_MAP.get(value, value))
            decoded["toolSwitches"] = decoded_list
        else:
            decoded[key] = value
    return decoded

if __name__ == "__main__":
    compressed_json = {
        "p": "eyeball",
        "s": True,
        "sh": ["cb", "sp"],
        "t": [[0, 0], [5, 1], [9, 3]],
    }

    example_gzip = base64.b64encode(
        gzip.compress(json.dumps(compressed_json).encode("utf-8"))
    ).decode("utf-8")

    print("Compressed JSON:", compressed_json)
    print("Gzipped & base64:", example_gzip)
    print("Decoded:", decode_session(example_gzip))
