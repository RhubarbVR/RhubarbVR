﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System.IO;
using RhuEngine.Linker;
using Assimp;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Collections.Generic;
using RNumerics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Assimp.Unmanaged;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public sealed partial class AssimpImporter : Importer
	{
		AssimpContext _assimpContext;

		public readonly SyncRef<IValueSource<bool>> setupIK;
		public readonly SyncRef<IValueSource<bool>> setupAvatarBones;
		private bool SetupIK => setupIK.Target?.Value ?? false;
		private bool SetupAvatarBones => setupAvatarBones.Target?.Value ?? false;

		public readonly SyncRef<IValueSource<bool>> srgbTextures;

		public readonly SyncRef<IValueSource<bool>> joinIdenticalVertices;
		public readonly SyncRef<IValueSource<bool>> makeLeftHanded;
		public readonly SyncRef<IValueSource<bool>> triangulate;
		public readonly SyncRef<IValueSource<bool>> generateNormals;
		public readonly SyncRef<IValueSource<bool>> generateSmoothNormals;
		public readonly SyncRef<IValueSource<bool>> splitLargeMeshes;
		public readonly SyncRef<IValueSource<bool>> preTransformVertices;
		public readonly SyncRef<IValueSource<bool>> limitBoneWeights;
		public readonly SyncRef<IValueSource<bool>> validateDataStructure;
		public readonly SyncRef<IValueSource<bool>> improveCacheLocality;
		public readonly SyncRef<IValueSource<bool>> removeRedundantMaterials;
		public readonly SyncRef<IValueSource<bool>> fixInFacingNormals;
		public readonly SyncRef<IValueSource<bool>> sortByPrimitiveType;
		public readonly SyncRef<IValueSource<bool>> findDegenerates;
		public readonly SyncRef<IValueSource<bool>> findInvalidData;
		public readonly SyncRef<IValueSource<bool>> generateUVCoords;
		public readonly SyncRef<IValueSource<bool>> transformUVCoords;
		public readonly SyncRef<IValueSource<bool>> findInstances;
		public readonly SyncRef<IValueSource<bool>> optimizeMeshes;
		public readonly SyncRef<IValueSource<bool>> optimizeGraph;
		public readonly SyncRef<IValueSource<bool>> flipUVs;
		public readonly SyncRef<IValueSource<bool>> flipWindingOrder;
		public readonly SyncRef<IValueSource<bool>> debone;
		public readonly SyncRef<IValueSource<bool>> forceGenerateNormals;
		public readonly SyncRef<IValueSource<bool>> dropNormals;

		public readonly SyncRef<IValueSource<bool>> removeNone;
		public readonly SyncRef<IValueSource<bool>> removeNormals;
		public readonly SyncRef<IValueSource<bool>> removeTangentBasis;
		public readonly SyncRef<IValueSource<bool>> removeColors;
		public readonly SyncRef<IValueSource<bool>> removeTexCoords;
		public readonly SyncRef<IValueSource<bool>> removeBoneweights;
		public readonly SyncRef<IValueSource<bool>> removeAnimations;
		public readonly SyncRef<IValueSource<bool>> removeTextures;
		public readonly SyncRef<IValueSource<bool>> removeLights;
		public readonly SyncRef<IValueSource<bool>> removeCameras;
		public readonly SyncRef<IValueSource<bool>> removeMeshes;
		public readonly SyncRef<IValueSource<bool>> removeMaterials;

		private bool RemoveNone => removeNone.Target?.Value ?? false;
		private bool RemoveNormals => removeNormals.Target?.Value ?? false;
		private bool RemoveTangentBasis => removeTangentBasis.Target?.Value ?? false;
		private bool RemoveColors => removeColors.Target?.Value ?? false;
		private bool RemoveTexCoords => removeTexCoords.Target?.Value ?? false;
		private bool RemoveBoneweights => removeBoneweights.Target?.Value ?? false;
		private bool RemoveAnimations => removeAnimations.Target?.Value ?? false;
		private bool RemoveTextures => removeTextures.Target?.Value ?? false;
		private bool RemoveLights => removeLights.Target?.Value ?? false;
		private bool RemoveCameras => removeCameras.Target?.Value ?? false;
		private bool RemoveMeshes => removeMeshes.Target?.Value ?? false;
		private bool RemoveMaterials => removeMaterials.Target?.Value ?? false;

		private bool SrgbTextures => srgbTextures.Target?.Value ?? false;
		private bool JoinIdenticalVertices => joinIdenticalVertices.Target?.Value ?? false;
		private bool MakeLeftHanded => makeLeftHanded.Target?.Value ?? false;
		private bool Triangulate => triangulate.Target?.Value ?? false;
		private bool GenerateNormals => generateNormals.Target?.Value ?? false;
		private bool GenerateSmoothNormals => generateSmoothNormals.Target?.Value ?? false;
		private bool SplitLargeMeshes => splitLargeMeshes.Target?.Value ?? false;
		private bool PreTransformVertices => preTransformVertices.Target?.Value ?? false;
		private bool LimitBoneWeights => limitBoneWeights.Target?.Value ?? false;
		private bool ValidateDataStructure => validateDataStructure.Target?.Value ?? false;
		private bool ImproveCacheLocality => improveCacheLocality.Target?.Value ?? false;
		private bool RemoveRedundantMaterials => removeRedundantMaterials.Target?.Value ?? false;
		private bool FixInFacingNormals => fixInFacingNormals.Target?.Value ?? false;
		private bool SortByPrimitiveType => sortByPrimitiveType.Target?.Value ?? false;
		private bool FindDegenerates => findDegenerates.Target?.Value ?? false;
		private bool FindInvalidData => findInvalidData.Target?.Value ?? false;
		private bool GenerateUVCoords => generateUVCoords.Target?.Value ?? false;
		private bool TransformUVCoords => transformUVCoords.Target?.Value ?? false;
		private bool FindInstances => findInstances.Target?.Value ?? false;
		private bool OptimizeMeshes => optimizeMeshes.Target?.Value ?? false;
		private bool OptimizeGraph => optimizeGraph.Target?.Value ?? false;
		private bool FlipUVs => flipUVs.Target?.Value ?? false;
		private bool FlipWindingOrder => flipWindingOrder.Target?.Value ?? false;
		private bool Debone => debone.Target?.Value ?? false;
		private bool ForceGenerateNormals => forceGenerateNormals.Target?.Value ?? false;
		private bool DropNormals => dropNormals.Target?.Value ?? false;


		public override void BuildUI(Entity rootBox) {
			base.BuildUI(rootBox);

			void AddCheckBox(string name, SyncRef<IValueSource<bool>> syncRef, bool defult) {
				var checkBOx = rootBox.AddChild(name).AttachComponent<CheckBox>();
				checkBOx.Text.Value = name;
				syncRef.Target = checkBOx.ButtonPressed;
				checkBOx.ButtonPressed.Value = defult;
			}
			AddCheckBox("Setup Avatar Bones", setupAvatarBones, true);
			AddCheckBox("Setup Avatar IK", setupIK, true);

			AddCheckBox("SRGB", srgbTextures, true);
			AddCheckBox("JoinIdenticalVertices", joinIdenticalVertices, false);
			AddCheckBox("MakeLeftHanded", makeLeftHanded, false);
			AddCheckBox("Triangulate", triangulate, false);
			AddCheckBox("GenerateNormals", generateNormals, false);
			AddCheckBox("GenerateSmoothNormals", generateSmoothNormals, false);
			AddCheckBox("SplitLargeMeshes", splitLargeMeshes, false);
			AddCheckBox("PreTransformVertices", preTransformVertices, false);
			AddCheckBox("LimitBoneWeights", limitBoneWeights, false);
			AddCheckBox("ValidateDataStructure", validateDataStructure, true);
			AddCheckBox("ImproveCacheLocality", improveCacheLocality, false);
			AddCheckBox("RemoveRedundantMaterials", removeRedundantMaterials, true);
			AddCheckBox("FixInFacingNormals", fixInFacingNormals, false);
			AddCheckBox("SortByPrimitiveType", sortByPrimitiveType, false);
			AddCheckBox("FindDegenerates", findDegenerates, false);
			AddCheckBox("FindInvalidData", findInvalidData, false);
			AddCheckBox("GenerateUVCoords", generateUVCoords, false);
			AddCheckBox("TransformUVCoords", transformUVCoords, false);
			AddCheckBox("FindInstances", findInstances, false);
			AddCheckBox("OptimizeMeshes", optimizeMeshes, false);
			AddCheckBox("OptimizeGraph", optimizeGraph, false);
			AddCheckBox("FlipUVs", flipUVs, true);
			AddCheckBox("FlipWindingOrder", flipWindingOrder, true);
			AddCheckBox("Debone", debone, false);
			AddCheckBox("ForceGenerateNormals", forceGenerateNormals, false);
			AddCheckBox("DropNormals", dropNormals, false);

			AddCheckBox("RemoveNormals", removeNormals, false);
			AddCheckBox("RemoveNone", removeNone, false);
			AddCheckBox("RemoveNormals", removeNormals, false);
			AddCheckBox("RemoveTangentBasis", removeTangentBasis, false);
			AddCheckBox("RemoveColors", removeColors, false);
			AddCheckBox("RemoveTexCoords", removeTexCoords, false);
			AddCheckBox("RemoveBoneweights", removeBoneweights, false);
			AddCheckBox("RemoveAnimations", removeAnimations, false);
			AddCheckBox("RemoveTextures", removeTextures, false);
			AddCheckBox("RemoveLights", removeLights, false);
			AddCheckBox("RemoveCameras", removeCameras, false);
			AddCheckBox("RemoveMeshes", removeMeshes, false);
			AddCheckBox("RemoveMaterials", removeMaterials, false);
		}


		public class StringArrayEqualityComparer : IEqualityComparer<string[]>
		{
			public bool Equals(string[] x, string[] y) {
				if (x.Length != y.Length) {
					return false;
				}
				for (var i = 0; i < x.Length; i++) {
					if (x[i] != y[i]) {
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(string[] obj) {
				var result = 17;
				for (var i = 0; i < obj.Length; i++) {
					unchecked {
						result = (result * 23) + obj[i].GetHashCode();
					}
				}
				return result;
			}
		}

		private class AssimpHolder
		{
			public Entity root;
			public Entity assetEntity;
			public Scene scene;

			public List<AssetProvider<RTexture2D>> textures = new();
			public List<ComplexMesh> meshes = new();
			public List<AssetProvider<RMaterial>> materials = new();
			public bool ReScale = true;
			public float TargetSize = 0.5f;
			public Dictionary<string, Entity> Nodes = new();
			public Dictionary<string, Armature> Armatures = new();
			public AxisAlignedBox3f BoundingBox = AxisAlignedBox3f.CenterZero;
			public List<(Entity, Node)> LoadMeshNodes = new();

			public AssimpHolder(Scene scene, Entity _root, Entity _assetEntity) {
				this.scene = scene;
				root = _root;
				assetEntity = _assetEntity;
			}

			public void CalculateOptimumBounds(ComplexMesh amesh, Entity entity) {
				var local = root.GlobalToLocal(entity.GlobalTrans);
				var mesh = BoundsUtil.Bounds(amesh.Vertices, (x) => x);
				mesh.Translate(local.Translation);
				mesh.Scale(local.Scale);
				BoundingBox = BoundsUtil.Combined(BoundingBox, mesh);
			}

			public void Rescale() {
				if (ReScale) {
					var size = BoundingBox.Extents;
					var largestSize = MathUtil.Max(size.x, size.y, size.z);
					root.scale.Value *= new Vector3f(TargetSize / largestSize);
				}
			}

			public void CalculateOptimumBounds(Entity entity) {
				var localPoint = root.GlobalPointToLocal(entity.GlobalTrans.Translation);
				BoundingBox = BoundsUtil.Combined(BoundingBox, new AxisAlignedBox3f { max = localPoint, min = localPoint });
			}
		}

		public override async Task ImportAsset() {
			try {
				Entity.rotation.Value *= Quaternionf.Pitched.Inverse;
				_assimpContext ??= new AssimpContext {
					Scale = .001f,
				};
				var postProcessSteps = PostProcessSteps.EmbedTextures;
				if (JoinIdenticalVertices) { postProcessSteps |= PostProcessSteps.JoinIdenticalVertices; }
				if (MakeLeftHanded) { postProcessSteps |= PostProcessSteps.MakeLeftHanded; }
				if (Triangulate) { postProcessSteps |= PostProcessSteps.Triangulate; }
				if (GenerateNormals) { postProcessSteps |= PostProcessSteps.GenerateNormals; }
				if (GenerateSmoothNormals) { postProcessSteps |= PostProcessSteps.GenerateSmoothNormals; }
				if (SplitLargeMeshes) { postProcessSteps |= PostProcessSteps.SplitLargeMeshes; }
				if (PreTransformVertices) { postProcessSteps |= PostProcessSteps.PreTransformVertices; }
				if (LimitBoneWeights) { postProcessSteps |= PostProcessSteps.LimitBoneWeights; }
				if (ValidateDataStructure) { postProcessSteps |= PostProcessSteps.ValidateDataStructure; }
				if (ImproveCacheLocality) { postProcessSteps |= PostProcessSteps.ImproveCacheLocality; }
				if (RemoveRedundantMaterials) { postProcessSteps |= PostProcessSteps.RemoveRedundantMaterials; }
				if (FixInFacingNormals) { postProcessSteps |= PostProcessSteps.FixInFacingNormals; }
				if (SortByPrimitiveType) { postProcessSteps |= PostProcessSteps.SortByPrimitiveType; }
				if (FindDegenerates) { postProcessSteps |= PostProcessSteps.FindDegenerates; }
				if (FindInvalidData) { postProcessSteps |= PostProcessSteps.FindInvalidData; }
				if (GenerateUVCoords) { postProcessSteps |= PostProcessSteps.GenerateUVCoords; }
				if (TransformUVCoords) { postProcessSteps |= PostProcessSteps.TransformUVCoords; }
				if (FindInstances) { postProcessSteps |= PostProcessSteps.FindInstances; }
				if (OptimizeMeshes) { postProcessSteps |= PostProcessSteps.OptimizeMeshes; }
				if (OptimizeGraph) { postProcessSteps |= PostProcessSteps.OptimizeGraph; }
				if (FlipUVs) { postProcessSteps |= PostProcessSteps.FlipUVs; }
				if (FlipWindingOrder) { postProcessSteps |= PostProcessSteps.FlipWindingOrder; }
				if (Debone) { postProcessSteps |= PostProcessSteps.Debone; }
				if (ForceGenerateNormals) { postProcessSteps |= PostProcessSteps.ForceGenerateNormals; }
				if (DropNormals) { postProcessSteps |= PostProcessSteps.DropNormals; }
				var excludeComponent = ExcludeComponent.None;
				if (RemoveNormals) { excludeComponent |= ExcludeComponent.Normals; }
				if (RemoveNone) { excludeComponent |= ExcludeComponent.None; };
				if (RemoveNormals) { excludeComponent |= ExcludeComponent.Normals; }
				if (RemoveTangentBasis) { excludeComponent |= ExcludeComponent.TangentBasis; }
				if (RemoveColors) { excludeComponent |= ExcludeComponent.Colors; }
				if (RemoveTexCoords) { excludeComponent |= ExcludeComponent.TexCoords; }
				if (RemoveBoneweights) { excludeComponent |= ExcludeComponent.Boneweights; }
				if (RemoveAnimations) { excludeComponent |= ExcludeComponent.Animations; }
				if (RemoveTextures) { excludeComponent |= ExcludeComponent.Textures; }
				if (RemoveLights) { excludeComponent |= ExcludeComponent.Lights; }
				if (RemoveCameras) { excludeComponent |= ExcludeComponent.Cameras; }
				if (RemoveMeshes) { excludeComponent |= ExcludeComponent.Meshes; }
				if (RemoveMaterials) { excludeComponent |= ExcludeComponent.Materials; }

				if (excludeComponent != ExcludeComponent.None) {
					postProcessSteps = PostProcessSteps.RemoveComponent;
					_assimpContext.SetConfig(new Assimp.Configs.IntegerPropertyConfig("PP_RVC_FLAGS", (int)excludeComponent));
				}
				Scene scene;
				if (_importData.isUrl) {
					using var client = new HttpClient();
					using var response = await client.GetAsync(_importData.url_path);
					using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
					scene = _assimpContext.ImportFileFromStream(streamToReadFrom, postProcessSteps, _importData.ex);
				}
				else {
					scene = _importData.rawData is null ? _assimpContext.ImportFile(_importData.url_path, postProcessSteps) : _assimpContext.ImportFileFromStream(_importData.rawData, postProcessSteps, _importData.ex);
				}
				if (scene is null) {
					RLog.Err("failed to Load Model Scene not loaded");
					return;
				}
				var root = Entity.AddChild("Root");
				var AssimpHolder = new AssimpHolder(scene, root, root.AddChild("Assets"));
				LoadTextures(AssimpHolder.assetEntity, AssimpHolder);
				LoadMaterials(AssimpHolder.assetEntity, AssimpHolder);
				LoadMesh(AssimpHolder.assetEntity, AssimpHolder);
				LoadNode(root, scene.RootNode, AssimpHolder);
				LoadLights(AssimpHolder.assetEntity, AssimpHolder);
				foreach (var item in AssimpHolder.LoadMeshNodes) {
					LoadMeshNode(item.Item1, item.Item2, AssimpHolder);
				}
				LoadAnimations(AssimpHolder.assetEntity, AssimpHolder);
				LoadCameras(AssimpHolder.assetEntity, AssimpHolder);
				AssimpHolder.Rescale();
				RLog.Info("Done Loading Model");
				if (SetupAvatarBones) {
					RLog.Info("Setting up Avatar Bones");
					var bones = new List<AvatarBones>();
					if (AssimpHolder.Armatures.Count == 0) {
						RLog.Err("Did not find Avatar");
						return;
					}
					foreach (var item in AssimpHolder.Armatures.Values) {
						bones.Add(item.Entity.AttachComponent<AvatarBones>());
					}
					RLog.Info($"Setting up {AssimpHolder.Armatures.Count} Avatars");
					foreach (var item in bones) {
						try {
							var errorMsg = item.SetupAvatar();
							if (errorMsg is not null) {
								RLog.Err($"Armature {item.Entity.name.Value} missing {errorMsg}");
							}
							else if (SetupIK) {
								item.SetupIK();
							}
						}
						catch (Exception e) {
							RLog.Err($"Armature {item.Entity.name.Value} Error: {e}");
						}

					}
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to Load Model Error {e}");
			}
		}

		private static void LoadNode(Entity ParrentEntity, Assimp.Node node, AssimpHolder scene) {
			RLog.Info($"Loaded Node {node.Name} Parrent {node.Parent?.Name ?? "NULL"}");
			var entity = ParrentEntity.AddChild(node.Name);
			entity.LocalTrans = Matrix.CreateFromAssimp(node.Transform);
			if (!scene.Nodes.ContainsKey(node.Name)) {
				scene.Nodes.Add(node.Name, entity);
			}
			if (node.HasChildren) {
				foreach (var item in node.Children) {
					LoadNode(entity, item, scene);
				}
			}
			scene.CalculateOptimumBounds(entity);
			if (node.HasMeshes) {
				scene.LoadMeshNodes.Add((entity, node));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		private static void LoadMesh(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasMeshes) {
				RLog.Info($"No Meshes");
				return;
			}
			foreach (var item in scene.scene.Meshes) {
				var newMesh = new ComplexMesh(item);
				if (item.MorphMethod == MeshMorphingMethod.None && newMesh.MeshAttachments.Count != 0) {
					newMesh.MorphingMethod = RMeshMorphingMethod.VertexBlend;
				}
				RLog.Info($"New Mesh MeshName:{newMesh.MeshName} MorphingMethod:{newMesh.MorphingMethod} VertexCount:{newMesh.VertexCount}  MeshAttachmentsCount:{newMesh.MeshAttachments.Count}");
				scene.meshes.Add(newMesh);
				RLog.Info($"Loaded Mesh {item.Name}");
			}
		}

		private static void LoadMaterials(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasMaterials) {
				RLog.Info($"No Materials");
				return;
			}
			foreach (var item in scene.scene.Materials) {
				if (item.IsPBRMaterial) {
					var mat = entity.AttachComponent<StandardMaterial>();
					scene.materials.Add(mat);
					if (item.HasShininess) {
						mat.Roughness.Value = item.Shininess;
					}
					if (item.HasColorDiffuse) {
						mat.AlbedoColor.Value = new Colorf(item.ColorDiffuse.R, item.ColorDiffuse.G, item.ColorDiffuse.B, item.ColorDiffuse.A);
					}
					if (item.HasTextureDiffuse) {
						try {
							mat.AlbedoTexture.Target = scene.textures[item.TextureDiffuse.TextureIndex];
						}
						catch {
							RLog.Err("Failed to load AlbedoTexture");
						}
					}
					if (item.HasTextureNormal) {
						var normalMapDeatere = entity.AttachComponent<NormalMapMaterialFeatere>();
						mat.NormalMap.Target = normalMapDeatere;
						try {
							normalMapDeatere.Texture.Target = scene.textures[item.TextureNormal.TextureIndex];
						}
						catch {
							RLog.Err("Failed to load normalMapDeatere Texture");
						}
					}
					if (item.HasTextureEmissive) {
						var emissionMaterialFeatere = entity.AttachComponent<EmissionMaterialFeatere>();
						mat.Emission.Target = emissionMaterialFeatere;
						try {
							emissionMaterialFeatere.Texture.Target = scene.textures[item.TextureEmissive.TextureIndex];
						}
						catch {
							RLog.Err("Failed to load emissionMaterialFeatere Texture");
						}
					}
					RLog.Info($"Loaded PBR Material");
				}
				else {
					var mat = entity.AttachComponent<UnlitMaterial>();
					scene.materials.Add(mat);
					if (item.HasColorDiffuse) {
						mat.Tint.Value = new Colorf(item.ColorDiffuse.R, item.ColorDiffuse.G, item.ColorDiffuse.B, item.ColorDiffuse.A);
					}
					if (item.HasTextureDiffuse) {
						try {
							mat.MainTexture.Target = scene.textures[item.TextureDiffuse.TextureIndex];
						}
						catch {
							RLog.Err("Failed to load MainTexture");
						}
					}
					RLog.Info($"Loaded Unlit Material");
				}
			}
		}

		private static void LoadLights(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasLights) {
				RLog.Info($"No lights");
				return;
			}
			var lights = entity.AddChild("Lights");
			foreach (var item in scene.scene.Lights) {
				var ligh = scene.Nodes.ContainsKey(item.Name) ? scene.Nodes[item.Name] : lights.AddChild(item.Name);
				Light3D light3D = null;
				switch (item.LightType) {
					case LightSourceType.Directional:
						var directionalLight3D = ligh.AttachComponent<DirectionalLight3D>();
						light3D = directionalLight3D;
						break;
					case LightSourceType.Point:
						var pointLight3D = ligh.AttachComponent<PointLight3D>();
						light3D = pointLight3D;
						break;
					case LightSourceType.Spot:
						var spotLight3D = ligh.AttachComponent<SpotLight3D>();
						light3D = spotLight3D;
						spotLight3D.Angle.Value = item.AngleInnerCone;
						break;
					default:
						RLog.Err("Did not know light type");
						break;
				}
				if (light3D is null) {
					return;
				}
				light3D.Color.Value = new Colorf(item.ColorDiffuse.R, item.ColorDiffuse.G, item.ColorDiffuse.B, 1);
			}
		}
		private void LoadTextures(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasTextures) {
				RLog.Info($"No Textures");
				return;
			}
			foreach (var item in scene.scene.Textures) {
				RLog.Info($"Loaded Texture {item.Filename}");
				if (item.HasCompressedData) {
					var newtexture = new ImageSharpTexture(new MemoryStream(item.CompressedData), SrgbTextures).CreateTextureAndDisposes();
					var textureURI = entity.World.CreateLocalAsset(newtexture);
					var tex = entity.AttachComponent<StaticTexture>();
					scene.textures.Add(tex);
					tex.url.Value = textureURI.ToString();
				}
				else if (item.HasNonCompressedData) {
					RLog.Err("not supported");
				}
				else {
					RLog.Err("Texture had no data to be found");
				}
			}
		}

		private static void LoadAnimations(Entity _, AssimpHolder scene) {
			if (!scene.scene.HasAnimations) {
				RLog.Info("No Animations");
				return;
			}
			foreach (var item in scene.scene.Animations) {
				RLog.Err($"Load Anim {item.Name} TicksPerSecond{item.TicksPerSecond}");
				RLog.Err($"MeshAnimationChannels {item.MeshAnimationChannels.Count}");
				RLog.Err($"MeshMorphAnimationChannels {item.MeshMorphAnimationChannels.Count}");
				RLog.Err($"NodeAnimationChannels {item.NodeAnimationChannels.Count}");
			}
		}

		private static void LoadCameras(Entity entity, AssimpHolder scene) {
			if (!scene.scene.HasCameras) {
				RLog.Info("No Cameras");
				return;
			}
			var camera = entity.AddChild("Cameras");
			foreach (var item in scene.scene.Cameras) {
				var newCamera = (scene.Nodes.ContainsKey(item.Name) ? scene.Nodes[item.Name] : camera.AddChild(item.Name)).AttachComponent<Camera3D>();
				newCamera.Near.Value = item.ClipPlaneNear;
				newCamera.Far.Value = item.ClipPlaneFar;
				newCamera.Perspective_Fov.Value = item.FieldOfview;
			}
		}

		private static void LoadMeshNode(Entity entity, Node node, AssimpHolder scene) {
			ComplexMesh complexMesh = null;
			var mits = new List<int>();
			foreach (var item in node.MeshIndices) {
				var rMesh = scene.meshes[item];
				var amesh = scene.scene.Meshes[item];
				if (complexMesh is not null) {
					try {
						complexMesh.AddSubMesh(rMesh);
						mits.Add(amesh.MaterialIndex);
					}
					catch {
						AddMeshRender(entity, node, scene, rMesh, new int[] { amesh.MaterialIndex });
					}
				}
				else {
					mits.Add(amesh.MaterialIndex);
					complexMesh = rMesh;
				}
			}
			if (complexMesh is not null) {
				AddMeshRender(entity, node, scene, complexMesh, mits);
			}
		}

		private static void AddMeshRender(Entity entity, Node node, AssimpHolder scene, ComplexMesh amesh, IEnumerable<int> mits) {
			RLog.Info($"Added Mesh Render SubMeshesCount:{amesh.SubMeshes.Count} VertexCount:{amesh.VertexCount} MeshAttachmentsCount:{amesh.MeshAttachments.Count} MeshBlendMode:{amesh.MorphingMethod}");
			var rmesh = scene.assetEntity.AttachComponent<StaticMesh>();
			if (amesh is not null) {
				rmesh.url.Value = entity.World.CreateLocalAsset(amesh).ToString();
			}
			if (amesh.HasBones || amesh.HasMeshAttachments) {
				Armature armiturer = null;
				if (amesh.HasBones) {
					scene.Nodes.TryGetValue(amesh.Bones[0].BoneName, out var armitureEntity);
					armiturer = armitureEntity?.parent.Target is not null
						? armitureEntity.parent.Target.GetFirstComponentOrAttach<Armature>()
						: entity.GetFirstComponentOrAttach<Armature>();
					if (armiturer.ArmatureEntitys.Count < amesh.Bones.Count) {
						foreach (var bone in amesh.Bones.Skip(armiturer.ArmatureEntitys.Count)) {
							if (scene.Nodes.ContainsKey(bone.Name)) {
								armiturer.ArmatureEntitys.Add().Target = scene.Nodes[bone.Name];
							}
							else {
								RLog.Info($"Didn't FindNode for {bone.Name}");
								var ent = entity.AddChild(bone.Name);
								armiturer.ArmatureEntitys.Add().Target = ent;
							}
						}
					}
				}
				scene.Armatures.TryAdd(armiturer.Entity.name.Value, armiturer);
				var meshRender = entity.AttachComponent<SkinnedMeshRender>();
				meshRender.Armature.Target = armiturer;
				foreach (var boneMesh in amesh.MeshAttachments) {
					var newShape = meshRender.BlendShapes.Add();
					newShape.BlendName.Value = boneMesh.Name;
					RLog.Info($"Added ShapeKey {newShape.BlendName.Value}");
				}
				meshRender.mesh.Target = rmesh;
				foreach (var item in mits) {
					meshRender.materials.Add().Target = scene.materials[item];
				}
			}
			else {
				scene.CalculateOptimumBounds(amesh, entity);
				var meshRender = entity.AttachComponent<MeshRender>();
				meshRender.mesh.Target = rmesh;
				foreach (var item in mits) {
					meshRender.materials.Add().Target = scene.materials[item];
				}
			}
			RLog.Info($"Added MeshNode {node.Name}");
		}


	}
}
