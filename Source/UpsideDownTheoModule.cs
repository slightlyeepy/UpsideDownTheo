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
	}

	public override void Unload() {
	}
}
