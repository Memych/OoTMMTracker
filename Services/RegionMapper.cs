using System.Collections.Generic;

namespace OoTMMTracker.Services
{
    public static class RegionMapper
    {
        // Mapping of regions from spoiler log to game locations
        public static readonly Dictionary<string, string> RegionMapping = new()
        {
            // Special regions
            { "Inside Eggs", "Special - Eggs" },
            { "Tingle", "Special - Tingle Maps" },
            { "Giant's Dream", "Special - Oath to Order" },
            { "Sacred Realm", "Temple of Time" },
            
            // OOT regions
            { "Kokiri Forest", "Kokiri Forest" },
            { "Lost Woods", "Lost Woods" },
            { "Sacred Forest Meadow", "Sacred Forest Meadow" },
            { "Hyrule Field", "Hyrule Field" },
            { "Hyrule Castle", "Hyrule Castle" },
            { "Outside Ganon's Castle", "Outside Ganon's Castle" },
            { "Market", "Market" },
            { "Temple of Time", "Temple of Time" },
            { "Kakariko", "Kakariko" },
            { "Graveyard", "Graveyard" },
            { "Death Mountain Trail", "Death Mountain Trail" },
            { "Death Mountain Crater", "Death Mountain Crater" },
            { "Goron City", "Goron City" },
            { "Zora's River", "Zora's River" },
            { "Zora's Domain", "Zora's Domain" },
            { "Zora's Fountain", "Zora's Fountain" },
            { "Lake Hylia", "Lake Hylia" },
            { "Gerudo Valley", "Gerudo Valley" },
            { "Gerudo's Fortress", "Gerudo's Fortress" },
            { "Thieves' Hideout", "Thieves' Hideout" },
            { "Haunted Wasteland", "Haunted Wasteland" },
            { "Desert Colossus", "Desert Colossus" },
            { "Lon Lon Ranch", "Lon Lon Ranch" },
            
            // OOT Dungeons
            { "Deku Tree", "Deku Tree" },
            { "Dodongo's Cavern", "Dodongo's Cavern" },
            { "Jabu-Jabu's Belly", "Jabu-Jabu's Belly" },
            { "Forest Temple", "Forest Temple" },
            { "Fire Temple", "Fire Temple" },
            { "Water Temple", "Water Temple" },
            { "Shadow Temple", "Shadow Temple" },
            { "Spirit Temple", "Spirit Temple" },
            { "Ice Cavern", "Ice Cavern" },
            { "Bottom of the Well", "Bottom of the Well" },
            { "Gerudo Training Grounds", "Gerudo Training Grounds" },
            { "Ganon's Castle", "Ganon's Castle" },
            { "Ganon's Tower", "Ganon's Tower" },
            
            // MM regions
            { "Termina Field", "Termina Field" },
            { "South Clock Town", "South Clock Town" },
            { "East Clock Town", "East Clock Town" },
            { "West Clock Town", "West Clock Town" },
            { "North Clock Town", "North Clock Town" },
            { "Laundry Pool", "Laundry Pool" },
            { "Milk Road", "Milk Road" },
            { "Romani Ranch", "Romani Ranch" },
            { "Road to Southern Swamp", "Road to Southern Swamp" },
            { "Southern Swamp", "Southern Swamp" },
            { "Deku Palace", "Deku Palace" },
            { "Woodfall", "Woodfall" },
            { "Mountain Village", "Mountain Village" },
            { "Twin Islands", "Twin Islands" },
            { "Goron Village", "Goron Village" },
            { "Path to Snowhead", "Path to Snowhead" },
            { "Snowhead", "Snowhead" },
            { "Great Bay Coast", "Great Bay Coast" },
            { "Zora Cape", "Zora Cape" },
            { "Zora Hall", "Zora Hall" },
            { "Pirate's Fortress", "Pirate's Fortress" },
            { "Road to Ikana", "Road to Ikana" },
            { "Ikana Canyon", "Ikana Canyon" },
            { "Ikana Graveyard", "Ikana Graveyard" },
            { "Stone Tower", "Stone Tower" },
            
            // MM Dungeons
            { "Woodfall Temple", "Woodfall Temple" },
            { "Snowhead Temple", "Snowhead Temple" },
            { "Great Bay Temple", "Great Bay Temple" },
            { "Stone Tower Temple", "Stone Tower Temple" },
            { "Inverted Stone Tower Temple", "Inverted Stone Tower Temple" },
            { "Swamp Spider House", "Swamp Spider House" },
            { "Ocean Spider House", "Ocean Spider House" },
            { "Pirate Fortress", "Pirate Fortress" },
            { "Beneath The Well", "Beneath The Well" },
            { "Ikana Castle", "Ikana Castle" },
            { "Secret Shrine", "Secret Shrine" },
            { "The Moon", "The Moon" },
        };
        
        public static string GetMappedRegion(string originalRegion)
        {
            if (RegionMapping.TryGetValue(originalRegion, out var mapped))
                return mapped;
            
            // If no mapping found, return original
            return originalRegion;
        }
    }
}
