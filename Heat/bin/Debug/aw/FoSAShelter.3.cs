using GTA;
using GTA.Math;
using GTA.UI;
using System;

namespace FoSA
{
	public class FoSAShelter : Script
	{
		private readonly Vector3[] entranceBoundingBox = { new Vector3(-489.6798f, 2233.712f, 149.539f), new Vector3(-488.1717f, 2235.22021f, 153.1024f) };
		private readonly Vector3[] exitBoundingBox = { new Vector3(-485.4304f, 2232.411f, 142.896088f), new Vector3(-484.308075f, 2233.533f, 144.036926f) };

		private const int fadeDuration = 300;
		private const int waitAfterTeleport = 1000;

		public FoSAShelter()
		{
			Tick += OnTick;
		}

		void Teleport(Ped ped, Vector3 position, float heading)
		{
			Screen.FadeOut(fadeDuration);
			Script.Wait(fadeDuration);

			ped.Position = position;
			ped.Heading = heading;

			Script.Wait(waitAfterTeleport);

			GameplayCamera.RelativeHeading = 0f;
			GameplayCamera.RelativePitch = 0f;

			Screen.FadeIn(fadeDuration);
		}

		bool IsTeleportable(Ped ped)
		{
			return ped.IsOnFoot && !ped.IsGettingIntoVehicle && !ped.IsJumpingOutOfVehicle;
		}

		void OnTick(object sender, EventArgs e)
		{
			Ped playerPed = Game.Player.Character;

			if (IsTeleportable(playerPed))
			{
				if (playerPed.IsInAngledArea(entranceBoundingBox[0], entranceBoundingBox[1], 2.2f))
				{
					Teleport(playerPed, new Vector3(-484.47f, 2230.11f, 143.95f), 185.30f);
				}
				else if (playerPed.IsInAngledArea(exitBoundingBox[0], exitBoundingBox[1], 1.0f))
				{
					Teleport(playerPed, new Vector3(-491.34f, 2235.70f, 150.87f), 98.05f);
				}
			}
		}
	}
}