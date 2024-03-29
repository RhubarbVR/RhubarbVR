﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Linker;

namespace RhubarbVR.Bindings.TextureBindings
{
	public class GodotTexture : IRTexture
	{
		public Texture Texture { get; set; }
		public RTexture RTexture { get; private set; }

		public void Dispose() {
			Texture?.Free();
			GC.SuppressFinalize(this);
		}

		public void Init(RTexture rTexture) {
			RTexture = rTexture;
			if(typeof(GodotTexture) == GetType()) {
				Texture = new Texture();
			}
		}
	}
}
