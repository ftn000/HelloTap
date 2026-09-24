import os

def create_script_meta(path, guid):
    meta_path = path + ".meta"
    content = f"fileFormatVersion: 2\nguid: {guid}\n"
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Created script meta: {meta_path}")

def create_audio_meta(path, guid):
    meta_path = path + ".meta"
    content = f"""fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 6
  defaultSettings:
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 0
    quality: 1
    conversionMode: 0
  platformSettingOverrides: {{}}
  forceToMono: 0
  normalize: 1
  preloadAudioData: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Created audio meta: {meta_path}")

def create_sprite_meta(path, guid):
    meta_path = path + ".meta"
    content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMasterTextureLimit: 0
  doesMipmapHaveMipmaps: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings: []
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Created sprite meta: {meta_path}")

def create_folder_meta(path, guid):
    meta_path = path + ".meta"
    content = f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Created folder meta: {meta_path}")

base = r"C:\HelloTap"

# Folders
create_folder_meta(os.path.join(base, "Assets", "Audio"), "4f1b7c4f1b2d26a4ea963703d6e0cb80")
create_folder_meta(os.path.join(base, "Assets", "Audio", "SFX"), "545b8658f528e7b42be608d107328d06")
create_folder_meta(os.path.join(base, "Assets", "Sprites"), "8ffe86859c487054dacf27814644c29f")
create_folder_meta(os.path.join(base, "Assets", "Scripts", "Audio"), "6f100000000000000000000000000001")
create_folder_meta(os.path.join(base, "Assets", "Scripts", "Editor"), "32de5eaf04635d54bb15e2ca3ae9042d")

# Scripts
create_script_meta(os.path.join(base, "Assets", "Scripts", "Audio", "AudioManager.cs"), "8a100000000000000000000000000001")
create_script_meta(os.path.join(base, "Assets", "Scripts", "UI", "WorkplaceVisuals.cs"), "8a100000000000000000000000000002")
create_script_meta(os.path.join(base, "Assets", "Scripts", "UI", "AudioToggleButton.cs"), "8a100000000000000000000000000003")
create_script_meta(os.path.join(base, "Assets", "Scripts", "Editor", "HelloTapSceneBuilder.cs"), "8a100000000000000000000000000004")
create_script_meta(os.path.join(base, "Assets", "Scripts", "Editor", "SpriteTexturePostprocessor.cs"), "8a100000000000000000000000000005")

# Audio
create_audio_meta(os.path.join(base, "Assets", "Audio", "SFX", "click_key1.wav"), "9a200000000000000000000000000001")
create_audio_meta(os.path.join(base, "Assets", "Audio", "SFX", "click_key2.wav"), "9a200000000000000000000000000002")
create_audio_meta(os.path.join(base, "Assets", "Audio", "SFX", "click_key3.wav"), "9a200000000000000000000000000003")
create_audio_meta(os.path.join(base, "Assets", "Audio", "SFX", "click_key4.wav"), "9a200000000000000000000000000004")
create_audio_meta(os.path.join(base, "Assets", "Audio", "SFX", "click_crit.wav"), "9a200000000000000000000000000005")
create_audio_meta(os.path.join(base, "Assets", "Audio", "SFX", "upgrade_buy.wav"), "9a200000000000000000000000000006")
create_audio_meta(os.path.join(base, "Assets", "Audio", "SFX", "project_release.wav"), "9a200000000000000000000000000007")
create_audio_meta(os.path.join(base, "Assets", "Audio", "SFX", "boost_activate.wav"), "9a200000000000000000000000000008")

# Sprites
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_desk_mat.png"), "7b300000000000000000000000000001")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_monitor_frame.png"), "7b300000000000000000000000000002")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_monitor_screen.png"), "7b300000000000000000000000000003")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_keyboard.png"), "7b300000000000000000000000000004")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_mouse.png"), "7b300000000000000000000000000005")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_coffee_mug.png"), "7b300000000000000000000000000006")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_energy_can.png"), "7b300000000000000000000000000007")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_cat.png"), "7b300000000000000000000000000008")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_card_bg.png"), "7b300000000000000000000000000009")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_btn_cyan.png"), "7b300000000000000000000000000010")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_btn_orange.png"), "7b300000000000000000000000000011")
create_sprite_meta(os.path.join(base, "Assets", "Sprites", "spr_btn_gold.png"), "7b300000000000000000000000000012")

print("All meta files generated successfully!")
