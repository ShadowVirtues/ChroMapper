using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.V3;
using Beatmap.V3.Customs;
using NUnit.Framework;
using SimpleJSON;

namespace TestsEditMode
{
    public class HeckRequirementsTestEditMode
    {
        // For use in PlayMode
        public void TestEverything()
        {
        }

        private V3Difficulty _difficulty;
        private BeatSaberSong.DifficultyBeatmap _info;

        private HeckRequirementCheck _chromaReq, _noodleReq;

        [OneTimeSetUp]
        public void SetupReqs()
        {
            _chromaReq = new ChromaReq();
            _noodleReq = new NoodleExtensionsReq();
        }

        [SetUp]
        public void SetupMop()
        {
            _difficulty = new V3Difficulty { MainNode = new JSONObject() };
            _info = new BeatSaberSong.DifficultyBeatmap(new BeatSaberSong.DifficultyBeatmapSet());
        }

        // Ensures the custom properties are filled with things from the custom data.
        // This is normally done in editor saving workflow but these tests aren't called from editor saving.
        private void RefreshCustomData()
        {
            _difficulty.Notes.ForEach(x => x.RefreshCustom());
            _difficulty.CustomEvents.ForEach(x => x.RefreshCustom());
        }

        [Test]
        public void UnusedTracksDoNotRequireMods()
        {
            _difficulty.Notes = new List<BaseNote>
            {
                new V3ColorNote
                {
                    CustomData = new JSONObject
                    {
                        ["track"] = "I am unused"
                    }
                }
            };
            _difficulty.CustomEvents = new List<BaseCustomEvent>
            {
                new V3CustomEvent
                {
                    Type = "AnimateTrack",
                    Data = new JSONObject
                    {
                        ["track"] = "1",
                        ["color"] = 0,
                        ["dissolve"] = 0,
                    }
                },
                new V3CustomEvent
                {
                    Type = "AssignPathAnimation",
                    Data = new JSONObject
                    {
                        ["track"] = "2",
                        ["color"] = 0,
                        ["dissolve"] = 0,
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.None, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.None, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }


        [TestCase("AnimateComponent")]
        public void TrackTypeAlwaysRequiresOnlyChroma(string trackType)
        {
            _difficulty.CustomEvents = new List<BaseCustomEvent>
            {
                new V3CustomEvent
                {
                    Type = trackType,
                    Data = new JSONObject
                    {
                        ["track"] = "3",
                        ["dissolve"] = 0
                    }
                }
            };

            RefreshCustomData();
            Assert.AreNotEqual(RequirementCheck.RequirementType.None, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.None, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }

        [TestCase("AssignTrackParent")]
        [TestCase("AssignPlayerToTrack")]
        public void TrackTypeAlwaysRequiresOnlyNoodle(string trackType)
        {
            _difficulty.CustomEvents = new List<BaseCustomEvent>
            {
                new V3CustomEvent
                {
                    Type = trackType,
                    Data = new JSONObject
                    {
                        ["track"] = "3",
                        ["color"] = 0
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.None, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.Requirement, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }
        
        [Test]
        public void AssignTrackParentAlwaysRequiresNoodle()
        {
            _difficulty.CustomEvents = new List<BaseCustomEvent>
            {
                new V3CustomEvent
                {
                    Type = "AssignTrackParent",
                    Data = new JSONObject
                    {
                        ["parentTrack"] = "parent",
                        ["childrenTracks"] = new JSONArray
                        {
                            [0] = "child"
                        }
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.None, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.Requirement, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }

        [TestCase("position", 0)]
        [TestCase("dissolve", 1)]
        [TestCase("interactable", 1)]
        public void TrackWithUsedNoodlePropertyRequiresNoodle(string property, dynamic value)
        {
            _difficulty.Notes = new List<BaseNote>
            {
                new V3ColorNote
                {
                    CustomData = new JSONObject
                    {
                        ["track"] = "3"
                    }
                }
            };
            _difficulty.CustomEvents = new List<BaseCustomEvent>
            {
                new V3CustomEvent
                {
                    Type = "AnimateTrack",
                    Data = new JSONObject
                    {
                        ["track"] = "3",
                        [property] = value
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.None, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.Requirement, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }

        [TestCase("color", 0)]
        public void TrackWithUsedChromaPropertySuggestsChroma(string property, dynamic value)
        {
            _difficulty.Notes = new List<BaseNote>
            {
                new V3ColorNote
                {
                    CustomData = new JSONObject
                    {
                        ["track"] = "3"
                    }
                }
            };
            _difficulty.CustomEvents = new List<BaseCustomEvent>
            {
                new V3CustomEvent
                {
                    Type = "AnimateTrack",
                    Data = new JSONObject
                    {
                        ["track"] = "3",
                        [property] = value
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.Suggestion, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.None, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }

        [TestCase("hafsdhklsdf", 0)]
        public void TrackWithGarbagePropertyRequiresNothing(string property, dynamic value)
        {
            _difficulty.Notes = new List<BaseNote>
            {
                new V3ColorNote
                {
                    CustomData = new JSONObject
                    {
                        ["track"] = "3"
                    }
                }
            };
            _difficulty.CustomEvents = new List<BaseCustomEvent>
            {
                new V3CustomEvent
                {
                    Type = "AnimateTrack",
                    Data = new JSONObject
                    {
                        ["track"] = "3",
                        [property] = value
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.None, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.None, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }

        [Test]
        public void TrackWithArrayWorks()
        {
            _difficulty.Notes = new List<BaseNote>
            {
                new V3ColorNote
                {
                    CustomData = new JSONObject
                    {
                        ["track"] = new JSONArray { [0] = "2", [1] = "3" }
                    }
                }
            };
            _difficulty.CustomEvents = new List<BaseCustomEvent>
            {
                new V3CustomEvent
                {
                    Type = "AnimateTrack",
                    Data = new JSONObject
                    {
                        ["track"] = "3",
                        ["color"] = 0,
                        ["dissolve"] = 0
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.Suggestion, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.Requirement, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }

        [Test]
        public void NoteWithColorAnimationSuggestsChroma()
        {
            _difficulty.Notes = new List<BaseNote>
            {
                new V3ColorNote
                {
                    CustomData = new JSONObject
                    {
                        ["animation"] = new JSONObject
                        {
                            ["color"] = 0
                        }
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.Suggestion, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.None, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }

        [TestCase("position", 0)]
        [TestCase("dissolve", 1)]
        [TestCase("interactable", 1)]
        public void NoteWithGameplayAnimationRequiresNoodle(string property, dynamic value)
        {
            _difficulty.Notes = new List<BaseNote>
            {
                new V3ColorNote
                {
                    CustomData = new JSONObject
                    {
                        ["animation"] = new JSONObject
                        {
                            [property] = value
                        }
                    }
                }
            };

            RefreshCustomData();
            Assert.AreEqual(RequirementCheck.RequirementType.None, _chromaReq.IsRequiredOrSuggested(_info, _difficulty));
            Assert.AreEqual(RequirementCheck.RequirementType.Requirement, _noodleReq.IsRequiredOrSuggested(_info, _difficulty));
        }
    }
}