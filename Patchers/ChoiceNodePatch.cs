using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using UnityEngine;
using HarmonyLib;
using System;
using Art = CostChoiceNodeFix.Resources.Artwork;

namespace CostChoiceNodeFix.Patchers
{
    internal static class ChoiceNodePatch
    {


		private static System.Random rng = new System.Random();

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}



		public static Texture2D LoadTextureFromResource(byte[] resourceFile)
		{
			var texture = new Texture2D(2, 2);
			texture.LoadImage(resourceFile);
			texture.filterMode = FilterMode.Point;
			return texture;
		}


		[HarmonyPatch(typeof(CardSingleChoicesSequencer), "GetCardbackTexture")]
		public class CardSingleChoicesSequencer_GetCardbackTexture
		{
			[HarmonyPostfix]
			public static void Postfix(ref Texture __result, CardChoice choice)
			{

				switch (choice.resourceType)
                {
					case ResourceType.Energy:
						if (Plugin.RenderFixActive)
                        {
							__result = LoadTextureFromResource(Art.renderCost);
						} else
                        {
							__result = LoadTextureFromResource(Art.energyCost);
						}
						break;
					case ResourceType.Gems:
						__result = LoadTextureFromResource(Art.moxCost);
						break;

                }

			}
		}

		[HarmonyPatch(typeof(Part1CardChoiceGenerator), "GenerateCostChoices")]
		public class Part1CardChoiceGenerator_GenerateCostChoices
		{
			[HarmonyPostfix]
			public static void Postfix(ref List<CardChoice> __result, int randomSeed)
			{
				var list = __result;

				if (GetRandomChoosableEnergyCard(SaveManager.SaveFile.GetCurrentRandomSeed()) != null)
                {
					CardChoice cardChoice1 = new CardChoice();
					cardChoice1.resourceType = ResourceType.Energy;
					list.Add(cardChoice1);
				}

				if (GetRandomChoosableMoxCard(SaveManager.SaveFile.GetCurrentRandomSeed()) != null)
				{
					CardChoice cardChoice2 = new CardChoice();
					cardChoice2.resourceType = ResourceType.Energy;
					list.Add(cardChoice2);
				}
				list.Shuffle();
				while (list.Count > 3)
				{
					list.RemoveAt(SeededRandom.Range(0, list.Count, randomSeed++));
				}
				__result = list;
			}
		}

		[HarmonyPatch(typeof(CardSingleChoicesSequencer))]
		public class ChoiceCostPatch
		{
			[HarmonyPostfix, HarmonyPatch(nameof(CardSingleChoicesSequencer.CostChoiceChosen))]
			public static IEnumerator PostfixGameLogicPatch(
			IEnumerator enumerator,
			CardSingleChoicesSequencer __instance,
			SelectableCard card
			)
			{
				if (card.ChoiceInfo.resourceType == ResourceType.Energy || card.ChoiceInfo.resourceType == ResourceType.Gems)
				{
					CardInfo cardInfo = new CardInfo();
					if (card.ChoiceInfo.resourceType == ResourceType.Energy)
					{
						cardInfo = GetRandomChoosableEnergyCard(SaveManager.SaveFile.GetCurrentRandomSeed());
					}
					if (card.ChoiceInfo.resourceType == ResourceType.Gems)
					{
						cardInfo = GetRandomChoosableMoxCard(SaveManager.SaveFile.GetCurrentRandomSeed());
					}
					card.SetInfo(cardInfo);
					card.SetFaceDown(false, false);
					card.SetInteractionEnabled(false);
					yield return __instance.TutorialTextSequence(card);		
					card.SetCardbackToDefault();
					yield return __instance.WaitForCardToBeTaken(card);
					yield break;
				} else
                {
					yield return enumerator;
				}
				
			}
		}

		public static CardInfo GetRandomChoosableEnergyCard(int randomSeed)
		{
			List<CardInfo> list = CardLoader.GetUnlockedCards(CardMetaCategory.ChoiceNode, CardTemple.Nature).FindAll((CardInfo x) => x.energyCost > 0);
			bool flag = list.Count == 0;
			CardInfo result;
			if (flag)
			{
				result = null;
			}
			else
			{
				result = CardLoader.Clone(list[SeededRandom.Range(0, list.Count, randomSeed)]);
			}
			return result;
		}

		public static CardInfo GetRandomChoosableMoxCard(int randomSeed)
		{
			List<CardInfo> list = CardLoader.GetUnlockedCards(CardMetaCategory.ChoiceNode, CardTemple.Nature).FindAll((CardInfo x) => x.gemsCost.Count > 0);
			bool flag = list.Count == 0;
			CardInfo result;
			if (flag)
			{
				result = null;
			}
			else
			{
				result = CardLoader.Clone(list[SeededRandom.Range(0, list.Count, randomSeed)]);
			}
			return result;
		}
	}
}
