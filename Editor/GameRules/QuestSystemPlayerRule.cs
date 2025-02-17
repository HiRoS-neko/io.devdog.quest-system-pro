﻿using Devdog.General;
using Devdog.General.Editors.GameRules;
using UnityEditor;
using UnityEngine;

namespace Devdog.QuestSystemPro.Editors
{
    public class QuestSystemPlayerRule : GameRuleBase
    {
        public override void UpdateIssue()
        {
            var players = Object.FindObjectsOfType<Player>();
            foreach (var player in players)
                CreateIssueIfMissingComponent<QuestSystemPlayer>(player.gameObject,
                    "Player component needs a QuestSystemPlayer component when using Quest System Pro",
                    MessageType.Error);
        }
    }
}