import sys
import requests
from mcp.server.fastmcp import FastMCP

mcp = FastMCP("UnityEditorBridge")

UNITY_URL = "http://localhost:8080"

@mcp.tool()
def get_scene_hierarchy() -> str:
    """Devuelve la lista de GameObjects raiz en la escena activa de Unity."""
    try:
        res = requests.get(f"{UNITY_URL}/hierarchy", timeout=3)
        if res.status_code == 200:
            return res.text
        return f"Error devuelto por Unity: {res.status_code}"
    except requests.exceptions.ConnectionError:
        return "No se pudo conectar con Unity. Revisa que Unity este abierto y el script UnityMcpServer activo en http://localhost:8080."

if __name__ == "__main__":
    mcp.run()