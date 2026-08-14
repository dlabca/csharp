import pathlib

def spoj_soubory(cesty, vystup="vysledek.txt"):
    with open(vystup, "w", encoding="utf-8") as out:
        for cesta in cesty:
            path = pathlib.Path(cesta)

            out.write(f"===== SOUBOR: {path} =====\n")
            try:
                with open(path, "r", encoding="utf-8") as f:
                    obsah = f.read()
                out.write(obsah + "\n\n")
            except Exception as e:
                out.write(f"[CHYBA] Soubor se nepodařilo načíst: {e}\n\n")

    print(f"Hotovo! Výstup je v: {vystup}")


# příklad použití:
cesty = [
    "P:\\open-world\\button.cs",
    "P:\\open-world\\chunk.cs",
    "P:\\open-world\\chunkManager.cs",
    "P:\\open-world\\ChunkWater.cs",
    "P:\\open-world\\DesktopShopUI.cs",
    "P:\\open-world\\DuckManager.cs",
    "P:\\open-world\\DuckMeshGenerator.cs",
    "P:\\open-world\\Game1.cs",
    "P:\\open-world\\GameEconomy.cs",
    "P:\\open-world\\Content\\DuckInstancing.fx",
    "P:\\open-world-android\\Activity1.cs",
    "P:\\open-world-android\\JoystickView.cs",
    "P:\\open-world-android\\NativeInput.cs"
]

spoj_soubory(cesty, "P:\\sloucene.txt")
