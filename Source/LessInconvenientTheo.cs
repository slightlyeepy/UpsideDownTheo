using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;

namespace Celeste.Mod.UpsideDownTheo {
	[CustomEntity("UpsideDownTheo/LessInconvenientTheo")]
	[TrackedAs(typeof(TheoCrystal))]
	public class LessInconvenientTheo : TheoCrystal {
		private Action<Scene> actorAdded;

		public LessInconvenientTheo(EntityData data, Vector2 offset) : base(data, offset) {
			// dirty hack to call base.base.Added() -- THIS CRASHES ON MACOS.
			actorAdded = (Action<Scene>)Activator.CreateInstance(typeof(Action<Scene>), this, typeof(Actor).GetMethod("Added").MethodHandle.GetFunctionPointer());
		}

		public override void Added(Scene scene) {
			actorAdded(scene);
			Level = SceneAs<Level>();
		}
	}
}
