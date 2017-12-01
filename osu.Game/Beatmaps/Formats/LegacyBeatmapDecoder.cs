﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyBeatmapDecoder : LegacyDecoder
    {
        private bool hasCustomColours;
        private ConvertHitObjectParser parser;

        private LegacySampleBank defaultSampleBank;
        private int defaultSampleVolume = 100;

        public LegacyBeatmapDecoder()
        {
        }

        public LegacyBeatmapDecoder(string header)
        {
            BeatmapVersion = int.Parse(header.Substring(17));
        }

        protected override void ProcessSection(Section section, string line)
        {
            switch (section)
            {
                case Section.General:
                    handleGeneral(line);
                    break;
                case Section.Editor:
                    handleEditor(line);
                    break;
                case Section.Metadata:
                    handleMetadata(line);
                    break;
                case Section.Difficulty:
                    handleDifficulty(line);
                    break;
                case Section.Events:
                    handleEvents(line);
                    break;
                case Section.TimingPoints:
                    handleTimingPoints(line);
                    break;
                case Section.Colours:
                    handleColours(line);
                    break;
                case Section.HitObjects:
                    handleHitObjects(line);
                    break;
                case Section.Variables:
                    handleVariables(line);
                    break;
            }
        }

        private void handleGeneral(string line)
        {
            var pair = splitKeyVal(line, ':');

            var metadata = Beatmap.BeatmapInfo.Metadata;
            switch (pair.Key)
            {
                case @"AudioFilename":
                    metadata.AudioFile = pair.Value;
                    break;
                case @"AudioLeadIn":
                    Beatmap.BeatmapInfo.AudioLeadIn = int.Parse(pair.Value);
                    break;
                case @"PreviewTime":
                    metadata.PreviewTime = int.Parse(pair.Value);
                    break;
                case @"Countdown":
                    Beatmap.BeatmapInfo.Countdown = int.Parse(pair.Value) == 1;
                    break;
                case @"SampleSet":
                    defaultSampleBank = (LegacySampleBank)Enum.Parse(typeof(LegacySampleBank), pair.Value);
                    break;
                case @"SampleVolume":
                    defaultSampleVolume = int.Parse(pair.Value);
                    break;
                case @"StackLeniency":
                    Beatmap.BeatmapInfo.StackLeniency = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"Mode":
                    Beatmap.BeatmapInfo.RulesetID = int.Parse(pair.Value);

                    switch (Beatmap.BeatmapInfo.RulesetID)
                    {
                        case 0:
                            parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser();
                            break;
                        case 1:
                            parser = new Rulesets.Objects.Legacy.Taiko.ConvertHitObjectParser();
                            break;
                        case 2:
                            parser = new Rulesets.Objects.Legacy.Catch.ConvertHitObjectParser();
                            break;
                        case 3:
                            parser = new Rulesets.Objects.Legacy.Mania.ConvertHitObjectParser();
                            break;
                    }
                    break;
                case @"LetterboxInBreaks":
                    Beatmap.BeatmapInfo.LetterboxInBreaks = int.Parse(pair.Value) == 1;
                    break;
                case @"SpecialStyle":
                    Beatmap.BeatmapInfo.SpecialStyle = int.Parse(pair.Value) == 1;
                    break;
                case @"WidescreenStoryboard":
                    Beatmap.BeatmapInfo.WidescreenStoryboard = int.Parse(pair.Value) == 1;
                    break;
            }
        }

        private void handleEditor(string line)
        {
            var pair = splitKeyVal(line, ':');

            switch (pair.Key)
            {
                case @"Bookmarks":
                    Beatmap.BeatmapInfo.StoredBookmarks = pair.Value;
                    break;
                case @"DistanceSpacing":
                    Beatmap.BeatmapInfo.DistanceSpacing = double.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"BeatDivisor":
                    Beatmap.BeatmapInfo.BeatDivisor = int.Parse(pair.Value);
                    break;
                case @"GridSize":
                    Beatmap.BeatmapInfo.GridSize = int.Parse(pair.Value);
                    break;
                case @"TimelineZoom":
                    Beatmap.BeatmapInfo.TimelineZoom = double.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
            }
        }

        private void handleMetadata(string line)
        {
            var pair = splitKeyVal(line, ':');

            var metadata = Beatmap.BeatmapInfo.Metadata;
            switch (pair.Key)
            {
                case @"Title":
                    metadata.Title = pair.Value;
                    break;
                case @"TitleUnicode":
                    metadata.TitleUnicode = pair.Value;
                    break;
                case @"Artist":
                    metadata.Artist = pair.Value;
                    break;
                case @"ArtistUnicode":
                    metadata.ArtistUnicode = pair.Value;
                    break;
                case @"Creator":
                    metadata.AuthorString = pair.Value;
                    break;
                case @"Version":
                    Beatmap.BeatmapInfo.Version = pair.Value;
                    break;
                case @"Source":
                    Beatmap.BeatmapInfo.Metadata.Source = pair.Value;
                    break;
                case @"Tags":
                    Beatmap.BeatmapInfo.Metadata.Tags = pair.Value;
                    break;
                case @"BeatmapID":
                    Beatmap.BeatmapInfo.OnlineBeatmapID = int.Parse(pair.Value);
                    break;
                case @"BeatmapSetID":
                    Beatmap.BeatmapInfo.OnlineBeatmapSetID = int.Parse(pair.Value);
                    metadata.OnlineBeatmapSetID = int.Parse(pair.Value);
                    break;
            }
        }

        private void handleDifficulty(string line)
        {
            var pair = splitKeyVal(line, ':');

            var difficulty = Beatmap.BeatmapInfo.BaseDifficulty;
            switch (pair.Key)
            {
                case @"HPDrainRate":
                    difficulty.DrainRate = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"CircleSize":
                    difficulty.CircleSize = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"OverallDifficulty":
                    difficulty.OverallDifficulty = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"ApproachRate":
                    difficulty.ApproachRate = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderMultiplier":
                    difficulty.SliderMultiplier = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderTickRate":
                    difficulty.SliderTickRate = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
            }
        }

        private void handleEvents(string line)
        {
            DecodeVariables(ref line);

            string[] split = line.Split(',');

            EventType type;
            if (!Enum.TryParse(split[0], out type))
                throw new InvalidDataException($@"Unknown event type {split[0]}");

            switch (type)
            {
                case EventType.Background:
                    string filename = split[2].Trim('"');
                    Beatmap.BeatmapInfo.Metadata.BackgroundFile = filename;
                    break;
                case EventType.Break:
                    var breakEvent = new BreakPeriod
                    {
                        StartTime = double.Parse(split[1], NumberFormatInfo.InvariantInfo),
                        EndTime = double.Parse(split[2], NumberFormatInfo.InvariantInfo)
                    };

                    if (!breakEvent.HasEffect)
                        return;

                    Beatmap.Breaks.Add(breakEvent);
                    break;
            }
        }

        private void handleTimingPoints(string line)
        {
            string[] split = line.Split(',');

            double time = double.Parse(split[0].Trim(), NumberFormatInfo.InvariantInfo);
            double beatLength = double.Parse(split[1].Trim(), NumberFormatInfo.InvariantInfo);
            double speedMultiplier = beatLength < 0 ? 100.0 / -beatLength : 1;

            TimeSignatures timeSignature = TimeSignatures.SimpleQuadruple;
            if (split.Length >= 3)
                timeSignature = split[2][0] == '0' ? TimeSignatures.SimpleQuadruple : (TimeSignatures)int.Parse(split[2]);

            LegacySampleBank sampleSet = defaultSampleBank;
            if (split.Length >= 4)
                sampleSet = (LegacySampleBank)int.Parse(split[3]);

            //SampleBank sampleBank = SampleBank.Default;
            //if (split.Length >= 5)
            //    sampleBank = (SampleBank)int.Parse(split[4]);

            int sampleVolume = defaultSampleVolume;
            if (split.Length >= 6)
                sampleVolume = int.Parse(split[5]);

            bool timingChange = true;
            if (split.Length >= 7)
                timingChange = split[6][0] == '1';

            bool kiaiMode = false;
            bool omitFirstBarSignature = false;
            if (split.Length >= 8)
            {
                int effectFlags = int.Parse(split[7]);
                kiaiMode = (effectFlags & 1) > 0;
                omitFirstBarSignature = (effectFlags & 8) > 0;
            }

            string stringSampleSet = sampleSet.ToString().ToLower();
            if (stringSampleSet == @"none")
                stringSampleSet = @"normal";

            DifficultyControlPoint difficultyPoint = Beatmap.ControlPointInfo.DifficultyPointAt(time);
            SoundControlPoint soundPoint = Beatmap.ControlPointInfo.SoundPointAt(time);
            EffectControlPoint effectPoint = Beatmap.ControlPointInfo.EffectPointAt(time);

            if (timingChange)
            {
                Beatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint
                {
                    Time = time,
                    BeatLength = beatLength,
                    TimeSignature = timeSignature
                });
            }

            if (speedMultiplier != difficultyPoint.SpeedMultiplier)
            {
                Beatmap.ControlPointInfo.DifficultyPoints.RemoveAll(x => x.Time == time);
                Beatmap.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint
                {
                    Time = time,
                    SpeedMultiplier = speedMultiplier
                });
            }

            if (stringSampleSet != soundPoint.SampleBank || sampleVolume != soundPoint.SampleVolume)
            {
                Beatmap.ControlPointInfo.SoundPoints.Add(new SoundControlPoint
                {
                    Time = time,
                    SampleBank = stringSampleSet,
                    SampleVolume = sampleVolume
                });
            }

            if (kiaiMode != effectPoint.KiaiMode || omitFirstBarSignature != effectPoint.OmitFirstBarLine)
            {
                Beatmap.ControlPointInfo.EffectPoints.Add(new EffectControlPoint
                {
                    Time = time,
                    KiaiMode = kiaiMode,
                    OmitFirstBarLine = omitFirstBarSignature
                });
            }
        }

        private void handleColours(string line)
        {
            var pair = splitKeyVal(line, ':');

            string[] split = pair.Value.Split(',');

            if (split.Length != 3)
                throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B): {pair.Value}");

            byte r, g, b;
            if (!byte.TryParse(split[0], out r) || !byte.TryParse(split[1], out g) || !byte.TryParse(split[2], out b))
                throw new InvalidOperationException(@"Color must be specified with 8-bit integer components");

            if (!hasCustomColours)
            {
                Beatmap.ComboColors.Clear();
                hasCustomColours = true;
            }

            // Note: the combo index specified in the beatmap is discarded
            if (pair.Key.StartsWith(@"Combo"))
            {
                Beatmap.ComboColors.Add(new Color4
                {
                    R = r / 255f,
                    G = g / 255f,
                    B = b / 255f,
                    A = 1f,
                });
            }
        }

        private void handleHitObjects(string line)
        {
            // If the ruleset wasn't specified, assume the osu!standard ruleset.
            if (parser == null)
                parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser();

            var obj = parser.Parse(line);

            if (obj != null)
                Beatmap.HitObjects.Add(obj);
        }

        private void handleVariables(string line)
        {
            var pair = splitKeyVal(line, '=');
            Variables[pair.Key] = pair.Value;
        }

        private KeyValuePair<string, string> splitKeyVal(string line, char separator)
        {
            var split = line.Trim().Split(new[] { separator }, 2);

            return new KeyValuePair<string, string>
            (
                split[0].Trim(),
                split.Length > 1 ? split[1].Trim() : string.Empty
            );
        }
    }
}
