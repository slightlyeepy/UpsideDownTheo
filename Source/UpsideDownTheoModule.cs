using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.UpsideDownTheo;

public class UpsideDownTheoModule : EverestModule {
	public static UpsideDownTheoModule Instance { get; private set; }

	public static bool MaddiesHelpingHandLoaded;

	public UpsideDownTheoModule() {
		Instance = this;
#if DEBUG
		// debug builds use verbose logging
		Logger.SetLogLevel("UpsideDownTheo", LogLevel.Verbose);
#else
		// release builds use info logging to reduce spam in log files
		Logger.SetLogLevel("UpsideDownTheo", LogLevel.Info);
#endif
	}

	public override void Load() {
		EverestModuleMetadata maddiesHelpingHand = new () {
			Name = "MaxHelpingHand",
			Version = new Version(1, 0, 0)
		};

		MaddiesHelpingHandLoaded = Everest.Loader.DependencyLoaded(maddiesHelpingHand);
		
		// hooks
		On.Celeste.Level.TransitionTo += transitionToHook;
		IL.Celeste.Level.EnforceBounds += enforceBoundsHook;
	}

	public override void Unload() {
		// hooks
		On.Celeste.Level.TransitionTo -= transitionToHook;
		IL.Celeste.Level.EnforceBounds -= enforceBoundsHook;
	}

	private static void transitionToHook(On.Celeste.Level.orig_TransitionTo orig, Level self, LevelData next, Vector2 direction) {
		Player player = self.Tracker.GetEntity<Player>();
		foreach (TheoCrystal crystal in self.Tracker.GetEntities<TheoCrystal>()) {
			if (crystal is UpsideDownTheo || crystal is LessInconvenientTheo) {
				Entity entity = null;
				if (player != null) {
					Holdable holding = player.Holding;
					entity = (holding != null) ? holding.Entity : null;
				}
				if (entity != crystal) {
					crystal.RemoveTag(Tags.TransitionUpdate);
				}
			}
		}
		orig(self, next, direction);
	}

	private static void enforceBoundsHook(ILContext il) {
		ILCursor cursor = new ILCursor(il);
		if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Tracker>("GetEntity"))) {
			cursor.EmitDelegate<Func<TheoCrystal, TheoCrystal>>(crystal => (crystal is UpsideDownTheo || crystal is LessInconvenientTheo) ? null : crystal);
		}
	}
}
