using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;

using Celeste.Mod.MaxHelpingHand.Entities;

namespace Celeste.Mod.UpsideDownTheo {
	[CustomEntity("UpsideDownTheo/UpsideDownTheo")]
	[TrackedAs(typeof(TheoCrystal))]
	public class UpsideDownTheo : TheoCrystal {
		private Action<Scene> actorAdded;

		public UpsideDownTheo(EntityData data, Vector2 offset) : base(data, offset) {
			// dirty hack to call base.base.Added() -- THIS CRASHES ON MACOS.
			actorAdded = (Action<Scene>)Activator.CreateInstance(typeof(Action<Scene>), this, typeof(Actor).GetMethod("Added").MethodHandle.GetFunctionPointer());

			Hold.OnHitSpinner = HitSpinnerUpsideDown;
			onCollideV = OnCollideVUpsideDown;

			sprite.Position.Y -= 10f;
			sprite.Scale.Y = -1f;
		}

		//[MonoModLinkTo("Celeste.Actor", "System.Void Added(Monocle.Scene)")]
		public override void Added(Scene scene) {
			actorAdded(scene);
			Level = SceneAs<Level>();
		}

		[MonoModLinkTo("Celeste.Actor", "System.Void Update()")]
		private void actorUpdate() {
			base.Update();
		}

		public override void Update() {
			actorUpdate();
			if (shattering || dead) {
				return;
			}
			if (swatTimer > 0f) {
				swatTimer -= Engine.DeltaTime;
			}
			hardVerticalHitSoundCooldown -= Engine.DeltaTime;
			if (OnPedestal) {
				base.Depth = 8999;
				return;
			}
			base.Depth = 100;
			if (Hold.IsHeld) {
				prevLiftSpeed = Vector2.Zero;
			} else {
				if (OnGroundUpsideDown()) {
					float target = ((!OnGroundUpsideDown(Position + Vector2.UnitX * 3f)) ? 20f : (OnGroundUpsideDown(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
					Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
					Vector2 liftSpeed = base.LiftSpeed;
					if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero) {
						Speed = prevLiftSpeed;
						prevLiftSpeed = Vector2.Zero;
						Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
						if (Speed.X != 0f && Speed.Y == 0f) {
							Speed.Y = -60f;
						}
						if (Speed.Y < 0f) {
							noGravityTimer = 0.15f;
						}
					} else {
						prevLiftSpeed = liftSpeed;
						if (liftSpeed.Y < 0f && Speed.Y < 0f) {
							Speed.Y = 0f;
						}
					}
				} else if (Hold.ShouldHaveGravity) {
					float num = 800f;
					if (Math.Abs(Speed.Y) <= 30f) {
						num *= 0.5f;
					}
					float num2 = 350f;
					if (Speed.Y < 0f) {
						num2 *= 0.5f;
					}
					Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
					if (noGravityTimer > 0f) {
						noGravityTimer -= Engine.DeltaTime;
					} else {
						Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
					}
				}
				previousPosition = base.ExactPosition;
				MoveH(Speed.X * Engine.DeltaTime, onCollideH);
				MoveV(-Speed.Y * Engine.DeltaTime, onCollideV);
				if (base.Center.X > (float)Level.Bounds.Right) {
					MoveH(32f * Engine.DeltaTime);
					if (base.Left - 8f > (float)Level.Bounds.Right) {
						RemoveSelf();
					}
				} else if (base.Left < (float)Level.Bounds.Left) {
					base.Left = Level.Bounds.Left;
					Speed.X *= -0.4f;
				} else if (base.Bottom > (float)(Level.Bounds.Bottom + 4)) {
					base.Bottom = Level.Bounds.Bottom - 4;
					Speed.Y = 0f;
				} else if (base.Top < (float)Level.Bounds.Top && SaveData.Instance.Assists.Invincible) {
					base.Top = Level.Bounds.Top;
					Speed.Y = -300f;
					Audio.Play("event:/game/general/assist_screenbottom", Position);
				} else if (base.Top < (float)Level.Bounds.Top) {
					Die();
				}
				if (base.X < (float)(Level.Bounds.Left + 10)) {
					MoveH(32f * Engine.DeltaTime);
				}
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				TempleGate templeGate = CollideFirst<TempleGate>();
				if (templeGate != null && entity != null) {
					templeGate.Collidable = false;
					MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
					templeGate.Collidable = true;
				}
			}
		}

		public void HitSpinnerUpsideDown(Entity spinner) {
			if (!Hold.IsHeld && Speed.Length() < 0.01f && base.LiftSpeed.Length() < 0.01f && (previousPosition - base.ExactPosition).Length() < 0.01f && OnGroundUpsideDown()) {
				int num = Math.Sign(base.X - spinner.X);
				if (num == 0) {
					num = 1;
				}
				Speed.X = (float)num * 120f;
				Speed.Y = -30f;
			}
		}

		public bool OnGroundUpsideDown(int upCheck = 1) {
			if (!CollideCheck<Solid>(Position - Vector2.UnitY * upCheck)) {
				if (!IgnoreJumpThrus && UpsideDownTheoModule.MaddiesHelpingHandLoaded) {
					return CollideCheckOutside<UpsideDownJumpThru>(Position - Vector2.UnitY * upCheck);
				}
				return false;
			}
			return true;
		}

		public bool OnGroundUpsideDown(Vector2 at, int upCheck = 1) {
			Vector2 position = Position;
			Position = at;
			bool result = OnGroundUpsideDown(upCheck);
			Position = position;
			return result;
		}

		private void OnCollideVUpsideDown(CollisionData data) {
			if (data.Hit is DashSwitch) {
				(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(-Speed.Y));
			}
			if (Speed.Y > 0f) {
				if (hardVerticalHitSoundCooldown <= 0f) {
					Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
					hardVerticalHitSoundCooldown = 0.5f;
				} else {
					Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
				}
			}
			if (Speed.Y > 160f) {
				ImpactParticles(data.Direction);
			}
			if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch)) {
				Speed.Y *= -0.6f;
			} else {
				Speed.Y = 0f;
			}
		}
	}
}
