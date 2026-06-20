using OoTMMTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoTMMTracker.Services
{
    public class MapLogicEvaluator : LogicMacros
    {
        public MapLogicEvaluator(Func<string, TrackerItem> getItem, Func<string, bool> isLocationFound, TrackerConfig config)
            : base(getItem, isLocationFound, config)
        {
        }

        public bool CanAccessLocation(string locationName)
        {
            string cleanName = locationName.Trim();

            switch (cleanName)
            {
                //Lost Woods
                //case "Lost Woods Butterfly 1":
                //    return IsChild() && HasOotSticks() && HasSoulOotButterflies();
                //case "Lost Woods Butterfly 2":
                //    return IsChild() && HasOotSticks() && HasSoulOotButterflies();
                //case "Lost Woods Butterfly 3":
                //    return IsChild() && HasOotSticks() && HasSoulOotButterflies();
                //case "Lost Woods Butterfly 4":
                //    return IsChild() && HasOotSticks() && HasSoulOotButterflies();
                //case "Lost Woods Butterfly 5":
                //    return IsChild() && HasOotSticks() && HasSoulOotButterflies();
                //case "Lost Woods Gift from Saria":
                //    return HasSoulOotSaria();
                //case "Lost Woods Memory Game":
                //    return CanPlayMinigame();
                //case "Lost Woods Pool Big Fairy":
                //    return CanPlayOotSongOfStorms();
                //case "Lost Woods Rupee Arrow 1":
                //    return IsChild() &&  (HasOotScale(1) || HasOotBoomerang());
                //case "Lost Woods Rupee Arrow 2":
                //    return IsChild() && (HasOotScale(1) || HasOotBoomerang());
                //case "Lost Woods Rupee Arrow 3":
                //    return IsChild() && (HasOotScale(1) || HasOotBoomerang());
                //case "Lost Woods Rupee Arrow 4":
                //    return IsChild() && (HasOotScale(1) || HasOotBoomerang());
                //case "Lost Woods Rupee Arrow 5":
                //    return IsChild() && (HasOotScale(1) || HasOotBoomerang());
                //case "Lost Woods Rupee Arrow 6":
                //    return IsChild() && (HasOotScale(1) || HasOotBoomerang());
                //case "Lost Woods Rupee Arrow 7":
                //    return IsChild() && (HasOotScale(1) || HasOotBoomerang());
                //case "Lost Woods Rupee Arrow 8":
                //    return IsChild() && (HasOotScale(1) || HasOotBoomerang());
                //case "Lost Woods Skull Kid":
                //    return IsChild() && CanPlayOotSariasSong();

                default:
                    return true;
            }
        }
        public bool CanAccessEntrance(string entranceId)
        {
            string cleanId = entranceId.Trim();

            switch (cleanId)
            {
                //Lost Woods
                //case "OOT_ZORA_RIVER_FROM_LOST_WOODS":
                //    return HasOotScale(1);

                default:
                    return true;
            }
        }
    }
}
