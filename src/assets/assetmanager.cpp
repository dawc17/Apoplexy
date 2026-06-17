#include "assetmanager.hpp"
#include "raylib.h"
#include "weapon/weapondata.hpp"
#include <raylib.h>

#include <vector>

namespace {
std::vector<int> latinFontCodepoints() {
  std::vector<int> codepoints;

  for (int c = 32; c <= 126; ++c) {
    codepoints.push_back(c);
  }

  return codepoints;
}

std::vector<int> japaneseFontCodepoints() {
  std::vector<int> codepoints;

  constexpr int japaneseCodepoints[] = {
      0x5F3E, // 弾
      0x85AC, // 薬
      0x72B6, // 状
      0x614B, // 態
      0x8B66, // 警
      0x544A, // 告
      0x8996, // 視
      0x8A8D, // 認
      0x691C, // 検
      0x77E5, // 知
      0x8074, // 聴
      0x97F3, // 音
      0x8A3A, // 診
      0x65AD, // 断
      0x7DDA, // 線
      0x5B8C, // 完
      0x4E86, // 了
  };

  for (int c : japaneseCodepoints) {
    codepoints.push_back(c);
  }

  return codepoints;
}
} // namespace

void AssetManager::load() {
  if (loaded) {
    return;
  }

  pistolModel = LoadModel("models/USP-S.obj");
  shotgunModel = LoadModel("models/Shotgun.glb");

  psxGlobalShader =
      LoadShader("shaders/psx_global.vs", "shaders/psx_global.fs");
  viewmodelShader = LoadShader("shaders/viewmodel.vs", "shaders/viewmodel.fs");
  worldLitShader = LoadShader("shaders/world_lit.vs", "shaders/world_lit.fs");
  muzzleFlashTexture = LoadTexture("textures/muzzleflash.png");

  if (FileExists("/usr/share/fonts/Adwaita/AdwaitaMono-Regular.ttf")) {
    std::vector<int> codepoints = latinFontCodepoints();
    terminalFont =
        LoadFontEx("/usr/share/fonts/Adwaita/AdwaitaMono-Regular.ttf", 96,
                   codepoints.data(), static_cast<int>(codepoints.size()));
    SetTextureFilter(terminalFont.texture, TEXTURE_FILTER_BILINEAR);
    terminalFontLoaded = true;
  } else {
    terminalFont = GetFontDefault();
  }

  if (FileExists("fonts/NotoSansJP-Regular.ttf")) {
    std::vector<int> codepoints = japaneseFontCodepoints();
    japaneseFont =
        LoadFontEx("fonts/NotoSansJP-Regular.ttf", 96, codepoints.data(),
                   static_cast<int>(codepoints.size()));
    SetTextureFilter(japaneseFont.texture, TEXTURE_FILTER_BILINEAR);
    japaneseFontLoaded = true;
  } else {
    japaneseFont = terminalFont;
  }

  if (FileExists("textures/USP-S.png")) {
    pistolTexture = LoadTexture("textures/USP-S.png");
    pistolTextureLoaded = true;
  }

  if (FileExists("textures/Shotgun.png")) {
    shotgunTexture = LoadTexture("textures/Shotgun.png");
    shotgunTextureLoaded = true;
  }

  if (FileExists("textures/skybox.png")) {
    skyboxShader = LoadShader("shaders/skybox.vs", "shaders/skybox.fs");
    skyboxShader.locs[SHADER_LOC_MAP_CUBEMAP] =
        GetShaderLocation(skyboxShader, "environmentMap");

    int environmentMap = MATERIAL_MAP_CUBEMAP;
    SetShaderValue(skyboxShader,
                   GetShaderLocation(skyboxShader, "environmentMap"),
                   &environmentMap, SHADER_UNIFORM_INT);

    Image skyboxImage = LoadImage("textures/skybox.png");
    skyboxCubemap = LoadTextureCubemap(skyboxImage, CUBEMAP_LAYOUT_AUTO_DETECT);
    UnloadImage(skyboxImage);

    skyboxModel = LoadModelFromMesh(GenMeshCube(1.0f, 1.0f, 1.0f));
    skyboxModel.materials[0].shader = skyboxShader;
    skyboxModel.materials[0].maps[MATERIAL_MAP_CUBEMAP].texture = skyboxCubemap;
    skyboxLoaded = true;
  }

  for (int i = 0; i < pistolModel.materialCount; ++i) {
    pistolModel.materials[i].shader = viewmodelShader;
    pistolModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].color = WHITE;

    if (pistolTextureLoaded) {
      pistolModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].texture =
          pistolTexture;
    }
  }

  for (int i = 0; i < shotgunModel.materialCount; ++i) {
    shotgunModel.materials[i].shader = viewmodelShader;
    shotgunModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].color = WHITE;

    if (shotgunTextureLoaded) {
      shotgunModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].texture =
          shotgunTexture;
    }
  }

  loaded = true;
}

void AssetManager::unload() {
  if (!loaded) {
    return;
  }

  UnloadModel(pistolModel);
  UnloadModel(shotgunModel);
  UnloadShader(psxGlobalShader);
  UnloadShader(viewmodelShader);
  UnloadShader(worldLitShader);
  UnloadTexture(muzzleFlashTexture);
  if (terminalFontLoaded) {
    UnloadFont(terminalFont);
    terminalFontLoaded = false;
  }
  if (japaneseFontLoaded) {
    UnloadFont(japaneseFont);
    japaneseFontLoaded = false;
  }
  if (pistolTextureLoaded) {
    UnloadTexture(pistolTexture);
    pistolTextureLoaded = false;
  }
  if (shotgunTextureLoaded) {
    UnloadTexture(shotgunTexture);
    shotgunTextureLoaded = false;
  }
  if (skyboxLoaded) {
    UnloadShader(skyboxShader);
    UnloadTexture(skyboxCubemap);
    UnloadModel(skyboxModel);
    skyboxLoaded = false;
  }

  loaded = false;
}

const Model &AssetManager::getWeaponModel(WeaponModelId modelId) const {
  switch (modelId) {
  case WeaponModelId::Shotgun:
    return shotgunModel;
  case WeaponModelId::Pistol:
  default:
    return pistolModel;
  }
}

Shader AssetManager::getPsxGlobalShader() const { return psxGlobalShader; }

Shader AssetManager::getViewmodelShader() const { return viewmodelShader; }

Shader AssetManager::getWorldLitShader() const { return worldLitShader; }

const Font &AssetManager::getTerminalFont() const { return terminalFont; }

const Font &AssetManager::getJapaneseFont() const { return japaneseFont; }

const Texture2D &AssetManager::getMuzzleFlashTexture() const {
  return muzzleFlashTexture;
}

const Model &AssetManager::getSkyboxModel() const { return skyboxModel; }

bool AssetManager::hasSkybox() const { return skyboxLoaded; }
