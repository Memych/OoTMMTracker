using OoTMMTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoTMMTracker.Services
{
    public class LogicMacros
    {
        private readonly Func<string, TrackerItem> _getItem;
        private readonly Func<string, bool> _isLocationFound;
        private readonly TrackerConfig _config;
        public LogicMacros(Func<string, TrackerItem> getItem, Func<string, bool> isLocationFound, TrackerConfig config)
        {
            _getItem = getItem;
            _isLocationFound = isLocationFound;
            _config = config;
        }
        private bool Has(string itemId)
        {
            var item = _getItem(itemId);
            if (item == null) return false;
            if (item.IsDungeonReward) return item.DungeonCleared;
            if (item.IsAutoKey) return item.AutoKeyLit;
            if (item.CollectedWhenFull) return item.CurrentCount >= item.MaxCount;
            if (item.PartialCollectedAt > 0) return item.CurrentCount >= item.PartialCollectedAt;
            return item.CurrentCount > 0;
        }
        private bool HasLevel(string itemId, int minLevel)
        {
            var item = _getItem(itemId);
            return item != null && item.CurrentCount >= minLevel;
        }
        private bool HasSong(string itemId)
        {
            var item = _getItem(itemId);
            if (item == null) return false;
            int requiredCount = (item.CollectedWhenFull && item.MaxCount > 1) ? item.MaxCount : 1;
            return item.CurrentCount >= requiredCount;
        }
        private bool IsSongComplete(string itemId)
        {
            var item = _getItem(itemId);
            if (item == null) return false;
            return item.CurrentCount >= item.MaxCount;
        }
        public bool HasSoulByName(string soulName)
        {
            if (string.IsNullOrEmpty(soulName)) return false;
            string soulId = SoulsData.ToId(soulName);
            return Has(soulId);
        }
        //Equipment
        public bool HasMasterSword()
        { return Has("oot_master_sword") || Has("sh_master_sword"); }
        public bool HasBiggoronSword()
        { return Has("oot_biggoron_sword") || Has("sh_biggoron_sword"); }
        public bool HasOotKokiriSword()
        { return Has("oot_sword") || Has("sh_sword"); }
        public bool HasMmKokiriSword()
        { return Has("mm_sword") || Has("sh_sword"); }
        public bool HasOotDekuShield()
        { return Has("oot_deku_shield") || Has("sh_deku_shield"); }
        public bool HasMmDekuShield()
        { return Has("mm_deku_shield") || Has("sh_deku_shield"); }
        public bool HasOotHylianShield()
        { return Has("oot_hylian_shield") || Has("sh_hylian_shield"); }
        public bool HasMmHeroShield()
        { return Has("mm_hero_shield") || Has("sh_hylian_shield"); }
        public bool HasOotMirrorShield()
        { return Has("oot_mirror_shield") || Has("sh_mirror_shield"); }
        public bool HasMmMirrorShield()
        { return Has("mm_mirror_shield") || Has("sh_mirror_shield"); }
        public bool HasGerudoCard()
        { return Has("oot_gerudo_card") || Has("sh_gerudo_card"); }
        public bool HasOotGoronTunic()
        { return Has("oot_goron_tunic") || Has("sh_goron_tunic"); }
        public bool HasMmGoronTunic()
        { return Has("mm_goron_tunic") || Has("oot_goron_tunic"); }
        public bool HasOotZoraTunic()
        { return Has("oot_zora_tunic") || Has("sh_zora_tunic"); }
        public bool HasMmZoraTunic()
        { return Has("mm_zora_tunic") || Has("sh_zora_tunic"); }
        public bool HasOotIronBoots()
        { return Has("oot_iron_boots") || Has("sh_iron_boots"); }
        public bool HasMmIronBoots()
        { return Has("mm_iron_boots") || Has("sh_iron_boots"); }
        public bool HasOotHoverBoots()
        { return Has("oot_hover_boots") || Has("sh_hover_boots"); }
        public bool HasMmHoverBoots()
        { return Has("mm_hover_boots") || Has("sh_hover_boots"); }
        public bool HasOotScale(int requiredLevel)
        {
            if (requiredLevel <= 0) return true;
            int actualRequiredLevel = requiredLevel + (_config.OotBronzeScale ? 1 : 0);
            if (_config.SharedScales)
            { return HasLevel("sh_scale", actualRequiredLevel); }
            else
            { return HasLevel("oot_scale", actualRequiredLevel); }
        }
        public bool HasMmScale(int requiredLevel)
        {
            if (requiredLevel <= 0) return true;
            int actualRequiredLevel = requiredLevel + (_config.OotBronzeScale ? 1 : 0);
            if (_config.SharedScales)
            { return HasLevel("sh_scale", actualRequiredLevel); }
            else
            { return HasLevel("mm_scale", actualRequiredLevel); }
        }
        public bool HasOotWallet()
        { return Has("oot_wallet") || Has("sh_wallet"); }
        public bool HasMmWallet()
        { return Has("mm_wallet") || Has("sh_wallet"); }
        public bool HasOotMagic()
        { return Has("oot_magic") || Has("sh_magic"); }
        public bool HasMmMagic()
        { return Has("mm_magic") || Has("sh_magic"); }
        //Items
        public bool HasOotSticks()
        { return Has("oot_deku_stick") || Has("sh_deku_stick"); }
        public bool HasMmSticks()
        { return Has("mm_deku_stick") || Has("sh_deku_stick"); }
        public bool HasOotNuts()
        { return Has("oot_deku_nut") || Has("sh_deku_nut"); }
        public bool HasMmNuts()
        { return Has("mm_deku_nut") || Has("sh_deku_nut"); }
        public bool HasOotBoomerang()
        { return Has("oot_boomerang") || Has("sh_boomerang"); }
        public bool HasMmBoomerang()
        { return Has("mm_boomerang") || Has("sh_boomerang"); }
        //Ocarina + Buttons
        public bool HasOotOcarina()
        { return Has("oot_ocarina") || Has("sh_ocarina"); }
        public bool HasMmOcarina()
        { return Has("mm_ocarina") || Has("sh_ocarina"); }
        public bool HasOotOcarinaOfTime()
        { return HasLevel("oot_ocarina", 2) || HasLevel("sh_ocarina", 2); }
        public bool HasMmOcarinaOfTime()
        { return HasLevel("mm_ocarina", 2) || HasLevel("sh_ocarina", 2); }
        public bool HasOotAButton()
        { return Has("sh_btn_a") || Has("oot_btn_a"); }
        public bool HasMmAButton()
        { return Has("sh_btn_a") || Has("mm_btn_a"); }
        public bool HasOotCUpButton()
        { return Has("sh_btn_cup") || Has("oot_btn_cup"); }
        public bool HasMmCUpButton()
        { return Has("sh_btn_cup") || Has("mm_btn_cup"); }
        public bool HasOotCDownButton()
        { return Has("sh_btn_cdown") || Has("oot_btn_cdown"); }
        public bool HasMmCDownButton()
        { return Has("sh_btn_cdown") || Has("mm_btn_cdown"); }
        public bool HasOotCLeftButton()
        { return Has("sh_btn_cleft") || Has("oot_btn_cleft"); }
        public bool HasMmCLeftButton()
        { return Has("sh_btn_cleft") || Has("mm_btn_cleft"); }
        public bool HasOotCRightButton()
        { return Has("sh_btn_cright") || Has("oot_btn_cright"); }
        public bool HasMmCRightButton()
        { return Has("sh_btn_cright") || Has("mm_btn_cright"); }
        public bool HasOotOcarinaButtonAny2_A()
        {
            return HasOotAButton() && (HasOotCUpButton() || HasOotCDownButton() ||
                   HasOotCLeftButton() || HasOotCRightButton());
        }
        public bool HasOotOcarinaButtonAny2_Up()
        {
            return HasOotCUpButton() && (HasOotCDownButton() ||
                   HasOotCLeftButton() || HasOotCRightButton());
        }
        public bool HasOotOcarinaButtonAny2_Down()
        { return HasOotCDownButton() && (HasOotCLeftButton() || HasOotCRightButton()); }
        public bool HasOotOcarinaButtonAny2_Left()
        { return HasOotCLeftButton() && HasOotCRightButton(); }
        public bool HasOotOcarinaButtonAny2()
        {
            return HasOotOcarinaButtonAny2_A() || HasOotOcarinaButtonAny2_Up() ||
                   HasOotOcarinaButtonAny2_Down() || HasOotOcarinaButtonAny2_Left();
        }
        public bool HasMmOcarinaButtonAny2_A()
        {
            return HasMmAButton() && (HasMmCUpButton() || HasMmCDownButton() ||
                   HasMmCLeftButton() || HasMmCRightButton());
        }
        public bool HasMmOcarinaButtonAny2_Up()
        {
            return HasMmCUpButton() && (HasMmCDownButton() ||
                   HasMmCLeftButton() || HasMmCRightButton());
        }
        public bool HasMmOcarinaButtonAny2_Down()
        { return HasMmCDownButton() && (HasMmCLeftButton() || HasMmCRightButton()); }
        public bool HasMmOcarinaButtonAny2_Left()
        { return HasMmCLeftButton() && HasMmCRightButton(); }
        public bool HasMmOcarinaButtonAny2()
        {
            return HasMmOcarinaButtonAny2_A() || HasMmOcarinaButtonAny2_Up() ||
                   HasMmOcarinaButtonAny2_Down() || HasMmOcarinaButtonAny2_Left();
        }
        //Has Songs
        public bool HasOotZeldaLullaby()
        { return HasSong("oot_zelda_lullaby") || HasSong("sh_zelda_lullaby"); }
        public bool HasMmZeldaLullaby()
        { return HasSong("mm_zelda_lullaby") || HasSong("sh_zelda_lullaby"); }
        public bool HasOotEponaSong()
        { return HasSong("oot_epona_song") || HasSong("sh_epona_song"); }
        public bool HasMmEponaSong()
        { return HasSong("mm_epona_song") || HasSong("sh_epona_song"); }
        public bool HasOotSariasSong()
        { return HasSong("oot_saria_song") || HasSong("sh_saria_song"); }
        public bool HasMmSariasSong()
        { return HasSong("mm_saria_song") || HasSong("sh_saria_song"); }
        public bool HasOotSunsSong()
        { return HasSong("oot_sun_song") || HasSong("sh_sun_song"); }
        public bool HasMmSunsSong()
        { return HasSong("mm_sun_song") || HasSong("sh_sun_song"); }
        public bool HasOotSongOfTime()
        { return HasSong("oot_song_of_time") || HasSong("sh_song_of_time"); }
        public bool HasMmSongOfTime()
        { return HasSong("mm_song_of_time") || HasSong("sh_song_of_time"); }
        public bool HasOotSongOfStorms()
        { return HasSong("oot_song_of_storms") || HasSong("sh_song_of_storms"); }
        public bool HasMmSongOfStorms()
        { return HasSong("mm_song_of_storms") || HasSong("sh_song_of_storms"); }
        public bool HasOotSongOfHealing()
        { return HasSong("oot_song_of_healing") || HasSong("sh_song_of_healing"); }
        public bool HasMmSongOfHealing()
        { return HasSong("mm_song_of_healing") || HasSong("sh_song_of_healing"); }
        public bool HasOotSongOfSoaring()
        { return HasSong("oot_song_of_soaring") || HasSong("sh_song_of_soaring"); }
        public bool HasMmSongOfSoaring()
        { return HasSong("mm_song_of_soaring") || HasSong("sh_song_of_soaring"); }
        public bool HasOotMinuetOfForest()
        { return HasSong("oot_minuet") || HasSong("sh_minuet"); }
        public bool HasMmMinuetOfForest()
        { return HasSong("mm_minuet") || HasSong("sh_minuet"); }
        public bool HasOotBoleroOfFire()
        { return HasSong("oot_bolero") || HasSong("sh_bolero"); }
        public bool HasMmBoleroOfFire()
        { return HasSong("mm_bolero") || HasSong("sh_bolero"); }
        public bool HasOotSerenadeOfWater()
        { return HasSong("oot_serenade") || HasSong("sh_serenade"); }
        public bool HasMmSerenadeOfWater()
        { return HasSong("mm_serenade") || HasSong("sh_serenade"); }
        public bool HasOotRequiemOfSpirit()
        { return HasSong("oot_requiem") || HasSong("sh_requiem"); }
        public bool HasMmRequiemOfSpirit()
        { return HasSong("mm_requiem") || HasSong("sh_requiem"); }
        public bool HasOotNocturneOfShadow()
        { return HasSong("oot_nocturne") || HasSong("sh_nocturne"); }
        public bool HasMmNocturneOfShadow()
        { return HasSong("mm_nocturne") || HasSong("sh_nocturne"); }
        public bool HasOotPreludeOfLight()
        { return HasSong("oot_prelude") || HasSong("sh_prelude"); }
        public bool HasMmPreludeOfLight()
        { return HasSong("mm_prelude") || HasSong("sh_prelude"); }
        public bool HasOotSonataOfAwakening()
        { return HasSong("oot_sonata") || HasSong("sh_sonata"); }
        public bool HasMmSonataOfAwakening()
        { return HasSong("mm_sonata") || HasSong("sh_sonata"); }
        public bool HasOotGoronLullabyFull()
        { return IsSongComplete("oot_goron_lullaby") || IsSongComplete("sh_goron_lullaby"); }
        public bool HasOotGoronLullabyIntro()
        { return Has("oot_goron_lullaby") || Has("sh_goron_lullaby"); }
        public bool HasMmGoronLullabyFull()
        { return IsSongComplete("mm_goron_lullaby") || IsSongComplete("sh_goron_lullaby"); }
        public bool HasMmGoronLullabyIntro()
        { return Has("mm_goron_lullaby") || Has("sh_goron_lullaby"); }
        public bool HasOotNewWaveBossaNova()
        { return HasSong("oot_new_wave") || HasSong("sh_new_wave"); }
        public bool HasMmNewWaveBossaNova()
        { return HasSong("mm_new_wave") || HasSong("sh_new_wave"); }
        public bool HasOotElegyOfEmptiness()
        { return HasSong("oot_elegy") || HasSong("sh_elegy"); }
        public bool HasMmElegyOfEmptiness()
        { return HasSong("mm_elegy") || HasSong("sh_elegy"); }
        public bool HasOotOathToOrder()
        { return HasSong("oot_oath") || HasSong("sh_oath"); }
        public bool HasMmOathToOrder()
        { return HasSong("mm_oath") || HasSong("sh_oath"); }
        public bool HasOotScarecrow()
        { return HasSong("oot_scarecrow") || HasSong("sh_scarecrow"); }
        public bool HasMmScarecrow()
        { return HasSong("mm_scarecrow") || HasSong("sh_scarecrow"); }
        //Can Play Songs
        public bool CanPlayOotZeldaLullaby()
        {
            return HasOotZeldaLullaby() && HasOotOcarina() && HasOotCUpButton() &&
                   HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayMmZeldaLullaby()
        {
            return HasMmZeldaLullaby() && HasMmOcarina() && HasMmCUpButton() &&
                   HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayOotEponaSong()
        {
            return HasOotEponaSong() && HasOotOcarina() && HasOotCUpButton() &&
                   HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayMmEpona()
        {
            return HasMmEponaSong() && HasMmOcarina() && HasMmCUpButton() &&
                   HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayOotSariasSong()
        {
            return HasOotSariasSong() && HasOotOcarina() && HasOotCDownButton() &&
                   HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayMmSariasSong()
        {
            return HasMmSariasSong() && HasMmOcarina() && HasMmCDownButton() &&
                   HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayOotSunsSong()
        {
            return HasOotSunsSong() && HasOotOcarina() && HasOotCDownButton() &&
                   HasOotCRightButton() && HasOotCUpButton();
        }
        public bool CanPlayMmSunsSong()
        {
            return HasMmSunsSong() && HasMmOcarina() && HasMmCDownButton() &&
                   HasMmCRightButton() && HasMmCUpButton();
        }
        public bool CanPlayOotSongOfTime()
        {
            return HasOotSongOfTime() && HasOotOcarina() && HasOotCDownButton() &&
                   HasOotCRightButton() && HasOotAButton();
        }
        public bool CanPlayMmSongOfTime()
        {
            return HasMmSongOfTime() && HasMmOcarina() && HasMmCDownButton() &&
                   HasMmCRightButton() && HasMmAButton();
        }
        public bool CanPlayOotSongOfStorms()
        {
            return HasOotSongOfStorms() && HasOotOcarina() && HasOotCDownButton() &&
                   HasOotCUpButton() && HasOotAButton();
        }
        public bool CanPlayMmSongOfStorms()
        {
            return HasMmSongOfStorms() && HasMmOcarina() && HasMmCDownButton() &&
                   HasMmCUpButton() && HasMmAButton();
        }
        public bool CanPlayOotMinuetOfForest()
        {
            return HasOotMinuetOfForest() && HasOotOcarina() && HasOotCUpButton() &&
                   HasOotCRightButton() && HasOotCLeftButton() && HasOotAButton();
        }
        public bool CanPlayMmMinuetOfForest()
        {
            return HasMmMinuetOfForest() && HasMmOcarina() && HasMmCUpButton() &&
                   HasMmCRightButton() && HasMmCLeftButton() && HasMmAButton();
        }
        public bool CanPlayOotBoleroOfFire()
        {
            return HasOotBoleroOfFire() && HasOotOcarina() && HasOotCDownButton() &&
                   HasOotCRightButton() && HasOotAButton();
        }
        public bool CanPlayMmBoleroOfFire()
        {
            return HasMmBoleroOfFire() && HasMmOcarina() && HasMmCDownButton() &&
                   HasMmCRightButton() && HasMmAButton();
        }
        public bool CanPlayOotSerenadeOfWater()
        {
            return HasOotSerenadeOfWater() && HasOotOcarina() && HasOotCDownButton() &&
                   HasOotCRightButton() && HasOotCLeftButton() && HasOotAButton();
        }
        public bool CanPlayMmSerenadeOfWater()
        {
            return HasMmSerenadeOfWater() && HasMmOcarina() && HasMmCDownButton() &&
                   HasMmCRightButton() && HasMmCLeftButton() && HasMmAButton();
        }
        public bool CanPlayOotNocturneOfShadow()
        {
            return HasOotNocturneOfShadow() && HasOotOcarina() && HasOotCDownButton() &&
                   HasOotCRightButton() && HasOotCLeftButton() && HasOotAButton();
        }
        public bool CanPlayMmNocturneOfShadow()
        {
            return HasMmNocturneOfShadow() && HasMmOcarina() && HasMmCDownButton() &&
                   HasMmCRightButton() && HasMmCLeftButton() && HasMmAButton();
        }
        public bool CanPlayOotPreludeOfLight()
        {
            return HasOotPreludeOfLight() && HasOotOcarina() && HasOotCUpButton() &&
                   HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayMmPreludeOfLight()
        {
            return HasMmPreludeOfLight() && HasMmOcarina() && HasMmCUpButton() &&
                   HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayOotSonataOfAwakening()
        {
            return HasOotSonataOfAwakening() && HasOotOcarina() && HasOotAButton() &&
                   HasOotCUpButton() && HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayMmSonataOfAwakening()
        {
            return HasMmSonataOfAwakening() && HasMmOcarina() && HasMmAButton() &&
                   HasMmCUpButton() && HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayOotGoronLullabyFull()
        {
            return HasOotGoronLullabyFull() && HasOotOcarina() && HasOotAButton() &&
                   HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayOotGoronLullabyIntro()
        {
            return HasOotGoronLullabyIntro() && HasOotOcarina() && HasOotAButton() &&
                   HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayMmGoronLullabyFull()
        {
            return HasMmGoronLullabyFull() && HasMmOcarina() && HasMmAButton() &&
                   HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayMmGoronLullabyIntro()
        {
            return HasMmGoronLullabyIntro() && HasMmOcarina() && HasMmAButton() &&
                   HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayOotNewWaveBossaNova()
        {
            return HasOotNewWaveBossaNova() && HasOotOcarina() && HasOotCUpButton() &&
                   HasOotCDownButton() && HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayMmNewWaveBossaNova()
        {
            return HasMmNewWaveBossaNova() && HasMmOcarina() && HasMmCUpButton() &&
                   HasMmCDownButton() && HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayOotElegyOfEmptiness()
        {
            return HasOotElegyOfEmptiness() && HasOotOcarina() && HasOotCUpButton() &&
                   HasOotCDownButton() && HasOotCRightButton() && HasOotCLeftButton();
        }
        public bool CanPlayMmElegyOfEmptiness()
        {
            return HasMmElegyOfEmptiness() && HasMmOcarina() && HasMmCUpButton() &&
                   HasMmCDownButton() && HasMmCRightButton() && HasMmCLeftButton();
        }
        public bool CanPlayOotOathToOrder()
        {
            return HasOotOathToOrder() && HasOotOcarina() && HasOotCUpButton() &&
                   HasOotCDownButton() && HasOotCRightButton() && HasOotAButton();
        }
        public bool CanPlayMmOathToOrder()
        {
            return HasMmOathToOrder() &&
                   HasMmOcarina() &&
                   HasMmCUpButton() &&
                   HasMmCDownButton() &&
                   HasMmCRightButton() &&
                   HasMmAButton();
        }
        public bool CanPlayOotScarecrow()
        {
            return (HasOotScarecrow() || (_config.FreeScarecrowOot))
                   && HasOotOcarina() && HasOotOcarinaButtonAny2();
        }
        public bool CanPlayMmScarecrow()
        {
            return (HasMmScarecrow() || (_config.FreeScarecrowMm))
                   && HasMmOcarina() && HasMmOcarinaButtonAny2();
        }
        public bool CanPlayMinigame()
        {
            return HasOotOcarina() && HasOotCUpButton() && HasOotCDownButton() &&
                   HasOotCRightButton() && HasOotCLeftButton() && HasOotAButton();
        }
        //Boss Souls
        public bool HasSoulQueenGohma()
        {
            if (!_config.SoulsBossOot) return true;
            return Has("soul_of_queen_gohma");
        }
        public bool HasSoulKingDodongo()
        {
            if (!_config.SoulsBossOot) return true;
            return Has("soul_of_king_dodongo");
        }
        public bool HasSoulBarinade()
        {
            if (!_config.SoulsBossOot) return true;
            return Has("soul_of_barinade");
        }
        public bool HasSoulPhantomGanon()
        {
            if (!_config.SoulsBossOot) return true;
            return Has("soul_of_phantom_ganon");
        }
        public bool HasSoulVolvagia()
        {
            if (!_config.SoulsBossOot) return true;
            return Has("soul_of_volvagia");
        }
        public bool HasSoulMorpha()
        {
            if (!_config.SoulsBossOot) return true;
            return Has("soul_of_morpha"); 
        }
        public bool HasSoulBongoBongo()
        {
            if (!_config.SoulsBossOot) return true;
            return Has("soul_of_bongo_bongo"); 
        }
        public bool HasSoulTwinrova()
        {
            if (!_config.SoulsBossOot) return true;
            return Has("soul_of_twinrova"); 
        }
        public bool HasSoulOdolwa()
        {
            if (!_config.SoulsBossMm) return true;
            return Has("soul_of_odolwa"); 
        }
        public bool HasSoulGoht()
        {
            if (!_config.SoulsBossMm) return true;
            return Has("soul_of_goht"); 
        }
        public bool HasSoulGyorg()
        {
            if (!_config.SoulsBossMm) return true;
            return Has("soul_of_gyorg");
        }
        public bool HasSoulTwinmold()
        {
            if (!_config.SoulsBossMm) return true;
            return Has("soul_of_twinmold");
        }
        public bool HasSoulIgos()
        {
            if (!_config.SoulsBossMm) return true;
            return Has("soul_of_igos");
        }
        //Enemy Souls
        public bool HasSoulOotAnubis()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_anubis_oot") || Has("soul_of_anubis"); 
        }
        public bool HasSoulOotArmos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_armos_oot") || Has("soul_of_armos");
        }
        public bool HasSoulMmArmos()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_armos_mm") || Has("soul_of_armos"); 
        }
        public bool HasSoulOotBabyDodongos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_baby_dodongos_oot") || Has("soul_of_baby_dodongos");
        }
        public bool HasSoulMmBadBats()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_bad_bats_mm") || Has("soul_of_bad_bats"); 
        }
        public bool HasSoulOotBeamos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_beamos_oot") || Has("soul_of_beamos"); 
        }
        public bool HasSoulMmBeamos()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_beamos_mm") || Has("soul_of_beamos"); 
        }
        public bool HasSoulMmBioBabas()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_bio_babas_mm") || Has("soul_of_bio_babas"); 
        }
        public bool HasSoulOotBirisBaris()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_biris_baris_oot") || Has("soul_of_biris_baris"); 
        }
        public bool HasSoulMmBoes()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_boes_mm") || Has("soul_of_boes"); 
        }
        public bool HasSoulOotBubbles()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_bubbles_oot") || Has("soul_of_bubbles"); 
        }
        public bool HasSoulMmBubbles()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_bubbles_mm") || Has("soul_of_bubbles"); 
        }
        public bool HasSoulMmCaptainKeeta()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_captain_keeta") || Has("soul_of_captain_keeta"); 
        }
        public bool HasSoulMmChuchus()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_chuchus_mm") || Has("soul_of_chuchus"); 
        }
        public bool HasSoulOotDarkLink()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_dark_link_oot") || Has("soul_of_dark_link"); 
        }
        public bool HasSoulOotDeadHands()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_dead_hands_oot") || Has("soul_of_dead_hands"); 
        }
        public bool HasSoulMmDeepPythons()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_deep_pythons_mm") || Has("soul_of_deep_pythons");
        }
        public bool HasSoulOotDekuBabas()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_deku_babas_oot") || Has("soul_of_deku_babas"); 
        }
        public bool HasSoulMmDekuBabas()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_deku_babas_mm") || Has("soul_of_deku_babas"); 
        }
        public bool HasSoulOotDekuScrubs()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_deku_scrubs_oot") || Has("soul_of_deku_scrubs"); 
        }
        public bool HasSoulMmDekuScrubs()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_deku_scrubs_mm") || Has("soul_of_deku_scrubs"); 
        }
        public bool HasSoulMmDexihands()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_dexihands_mm") || Has("soul_of_dexihands"); 
        }
        public bool HasSoulOotDodongos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_dodongos_oot") || Has("soul_of_dodongos"); 
        }
        public bool HasSoulMmDodongos()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_dodongos_mm") || Has("soul_of_dodongos"); 
        }
        public bool HasSoulMmDragonflies()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_dragonflies_mm") || Has("soul_of_dragonflies");
        }
        public bool HasSoulMmEenoes()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_eenoes_mm") || Has("soul_of_eenoes"); 
        }
        public bool HasSoulMmEyegores()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_eyegores_mm") || Has("soul_of_eyegores");
        }
        public bool HasSoulOotFightingGerudos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_the_fighting_gerudos_oot") || Has("soul_of_the_fighting_thieves"); 
        }
        public bool HasSoulMmFightingPirates()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_the_fighting_pirates_mm") || Has("soul_of_the_fighting_thieves"); 
        }
        public bool HasSoulOotFlareDancers()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_flare_dancers_oot") || Has("soul_of_flare_dancers"); 
        }
        public bool HasSoulOotFloormasters()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_floormasters_oot") || Has("soul_of_floormasters"); 
        }
        public bool HasSoulMmFloormasters()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_floormasters_mm") || Has("soul_of_floormasters"); 
        }
        public bool HasSoulOotFlyingPots()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_flying_pots_oot") || Has("soul_of_flying_pots"); 
        }
        public bool HasSoulMmFlyingPots()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_flying_pots_mm") || Has("soul_of_flying_pots"); 
        }
        public bool HasSoulOotFreezards()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_freezards_oot") || Has("soul_of_freezards"); 
        }
        public bool HasSoulMmFreezards()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_freezards_mm") || Has("soul_of_freezards"); 
        }
        public bool HasSoulMmGaro()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_garo_mm") || Has("soul_of_garo"); 
        }
        public bool HasSoulMmGekkos()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_gekkos_mm") || Has("soul_of_gekkos"); 
        }
        public bool HasSoulOotGohmaLarvaes()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_gohma_larvaes_oot") || Has("soul_of_gohma_larvaes"); 
        }
        public bool HasSoulMmGomess()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_gomess_mm") || Has("soul_of_gomess"); 
        }
        public bool HasSoulOotGuays()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_guays_oot") || Has("soul_of_guays"); 
        }
        public bool HasSoulMmGuays()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_guays_mm") || Has("soul_of_guays"); 
        }
        public bool HasSoulMmHiploops()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_hiploops_mm") || Has("soul_of_hiploops"); 
        }
        public bool HasSoulOotIronKnuckles()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_iron_knuckles_oot") || Has("soul_of_iron_knuckles"); 
        }
        public bool HasSoulMmIronKnuckles()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_iron_knuckles_mm") || Has("soul_of_iron_knuckles"); 
        }
        public bool HasSoulOotJabuJabusParasites()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_jabu_jabus_parasites_oot") || Has("soul_of_jabu_jabus_parasites"); 
        }
        public bool HasSoulOotKeese()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_keese_oot") || Has("soul_of_keese"); 
        }
        public bool HasSoulMmKeese()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_keese_mm") || Has("soul_of_keese"); 
        }
        public bool HasSoulOotLeevers()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_leevers_oot") || Has("soul_of_leevers"); 
        }
        public bool HasSoulMmLeevers()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_leevers_mm") || Has("soul_of_leevers"); 
        }
        public bool HasSoulOotLikeLikes()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_like_likes_oot") || Has("soul_of_like_likes"); 
        }
        public bool HasSoulMmLikeLikes()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_like_likes_mm") || Has("soul_of_like_likes"); 
        }
        public bool HasSoulOotLizalfosDinolfos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_lizalfos_dinolfos_oot") || Has("soul_of_lizalfos_dinolfos"); 
        }
        public bool HasSoulMmLizalfosDinolfos()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_lizalfos_dinolfos_mm") || Has("soul_of_lizalfos_dinolfos"); 
        }
        public bool HasSoulOotMoblins()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_moblins_oot") || Has("soul_of_moblins"); 
        }
        public bool HasSoulMmNejirons()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_nejirons_mm") || Has("soul_of_lizalfos_nejirons"); 
        }
        public bool HasSoulOotOctoroks()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_octoroks_oot") || Has("soul_of_octoroks"); 
        }
        public bool HasSoulMmOctoroks()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_octoroks_mm") || Has("soul_of_octoroks"); 
        }
        public bool HasSoulOotPeahats()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_peahats_oot") || Has("soul_of_peahats"); 
        }
        public bool HasSoulMmPeahats()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_peahats_mm") || Has("soul_of_peahats"); 
        }
        public bool HasSoulOotPoes()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_poes_oot") || Has("soul_of_poes"); 
        }
        public bool HasSoulMmPoes()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_poes_mm") || Has("soul_of_poes"); 
        }
        public bool HasSoulMmRealBombchu()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_real_bombchu_mm") || Has("soul_of_real_bombchu"); 
        }
        public bool HasSoulOotReDeadsGibdos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_redeads_gibdos_oot") || Has("soul_of_redeads_gibdos"); 
        }
        public bool HasSoulMmReDeadsGibdos()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_redeads_gibdos_mm") || Has("soul_of_redeads_gibdos"); 
        }
        public bool HasSoulOotShaboms()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_shadoms_oot") || Has("soul_of_shaboms"); 
        }
        public bool HasSoulOotShellBlades()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_shell_blades_oot") || Has("soul_of_shell_blades"); 
        }
        public bool HasSoulMmShellBlades()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_shell_blades_mm") || Has("soul_of_shell_blades"); 
        }
        public bool HasSoulOotSkullKids()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_skull_kids_oot") || Has("soul_of_skull_kids"); 
        }
        public bool HasSoulMmSkullfish()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_skullfish_mm") || Has("soul_of_skullfish"); 
        }
        public bool HasSoulOotSkulltulas()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_skulltulas_oot") || Has("soul_of_skulltulas"); 
        }
        public bool HasSoulMmSkulltulas()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_skulltulas_mm") || Has("soul_of_skulltulas"); 
        }
        public bool HasSoulOotSkullwalltulas()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_skullwalltulas_oot") || Has("soul_of_skullwalltulas"); 
        }
        public bool HasSoulMmSkullwalltulas()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_skullwalltulas_mm") || Has("soul_of_skullwalltulas"); 
        }
        public bool HasSoulMmSnappers()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_snappers_mm") || Has("soul_of_snappers"); 
        }
        public bool HasSoulOotSpikes()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_spikes_oot") || Has("soul_of_spikes"); 
        }
        public bool HasSoulOotStalchildren()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_stalchildren_oot") || Has("soul_of_stalchildren"); 
        }
        public bool HasSoulMmStalchildren()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_stalchildren_mm") || Has("soul_of_stalchildren"); 
        }
        public bool HasSoulOotStalfos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_stalfos_oot") || Has("soul_of_stalfos"); 
        }
        public bool HasSoulOotStingers()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_stingers_oot") || Has("soul_of_stingers"); 
        }
        public bool HasSoulOotTailpasarans()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_tailpasarans_oot") || Has("soul_of_tailpasarans"); 
        }
        public bool HasSoulMmTakkuri()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_takkuri_mm") || Has("soul_of_takkuri"); 
        }
        public bool HasSoulOotTektites()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_tektites_oot") || Has("soul_of_tektites"); 
        }
        public bool HasSoulMmTektites()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_tektites_mm") || Has("soul_of_tektites"); 
        }
        public bool HasSoulOotTorchSlugs()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_torch_slugs_oot") || Has("soul_of_torch_slugs"); 
        }
        public bool HasSoulOotWallmasters()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_wallmasters_oot") || Has("soul_of_wallmasters"); 
        }
        public bool HasSoulMmWallmasters()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_wallmasters_mm") || Has("soul_of_wallmasters"); 
        }
        public bool HasSoulMmWarts()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_warts_mm") || Has("soul_of_warts"); 
        }
        public bool HasSoulMmWizzrobes()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_wizzrobes_mm") || Has("soul_of_wizzrobes"); 
        }
        public bool HasSoulOotWolfos()
        {
            if (!_config.SoulsEnemyOot && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_wolfos_oot") || Has("soul_of_wolfos"); 
        }
        public bool HasSoulMmWolfos()
        {
            if (!_config.SoulsEnemyMm && !_config.SharedSoulsEnemy) return true;
            return Has("soul_of_wolfos_mm") || Has("soul_of_wolfos"); 
        }
        //NPC Souls
        public bool HasSoulOotAstronomer()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_astronomer_oot") || Has("soul_of_astronomer"); 
        }
        public bool HasSoulMmAstronomer()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_astronomer_mm") || Has("soul_of_astronomer"); 
        }
        public bool HasSoulOotBazaarShopkeeper()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_bazaar_Shopkeeper_oot") || Has("soul_of_the_bazaar_swamp_archery_owner"); 
        }
        public bool HasSoulMmSwampArcheryOwner()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_swamp_archery_owner_mm") || Has("soul_of_the_bazaar_swamp_archery_owner"); 
        }
        public bool HasSoulOotBeanSalesman()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_bean_salesman_oot") || Has("soul_of_bean_salesman"); 
        }
        public bool HasSoulMmBeansSalesman()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_beans_salesman_mm") || Has("soul_of_bean_salesman"); 
        }
        public bool HasSoulOotBeggar()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_beggar_oot") || Has("soul_of_the_beggar_banker"); 
        }
        public bool HasSoulMmBanker()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_banker_mm") || Has("soul_of_the_beggar_banker"); 
        }
        public bool HasSoulOotBiggoron()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_biggoron_oot") || Has("soul_of_biggoron"); 
        }
        public bool HasSoulMmBiggoron()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_biggoron_mm") || Has("soul_of_biggoron"); 
        }
        public bool HasSoulMmBlacksmiths()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_blacksmiths_mm") || Has("soul_of_blacksmiths"); 
        }
        public bool HasSoulMmTouristCenterOwner()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_tuorist_center_owner_mm") || Has("soul_of_tourist_center_owner"); 
        }
        public bool HasSoulOotBombchuShopkeeper()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_bombchu_shopkeeper_oot") || Has("soul_of_the_bombchu_bomb_shop_owner");
        }
        public bool HasSoulMmBombShopOwner()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_bomb_shop_owner_mm") || Has("soul_of_the_bombchu_bomb_shop_owner");
        }
        public bool HasSoulOotBombchuBowlingLady()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_bombchu_bowling_lady_oot") || Has("soul_of_bombchu_bowling_chest_game_lady"); 
        }
        public bool HasSoulMmChestGameLady()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_chest_game_lady_mm") || Has("soul_of_bombchu_bowling_chest_game_lady"); 
        }
        public bool HasSoulOotCarpenters()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_carpenters_oot") || Has("soul_of_carpenters");
        }
        public bool HasSoulMmCarpenters()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_carpenters_mm") || Has("soul_of_carpenters"); 
        }
        public bool HasSoulOotCarpetMan()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_carpet_man_oot") || Has("soul_of_the_carpet_man_swordsman");
        }
        public bool HasSoulMmSwordsman()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_swordsman_mm") || Has("soul_of_the_carpet_man_swordsman"); 
        }
        public bool HasSoulOotChestGameOwner()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_chest_game_owner_oot") || Has("soul_of_the_chest_game_owner_fisherman"); 
        }
        public bool HasSoulMmFisherman()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_fisherman_mm") || Has("soul_of_the_chest_game_owner_fisherman"); 
        }
        public bool HasSoulOotGoronChild()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_goron_child_oot") || Has("soul_of_the_goron_child_baby"); 
        }
        public bool HasSoulMmGoronBaby()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_goron_baby_mm") || Has("soul_of_the_goron_child_baby"); 
        }
        public bool HasSoulOotCitizens()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_citizens_oot") || Has("soul_of_citizens"); 
        }
        public bool HasSoulMmCitizens()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_citizens_oot") || Has("soul_of_citizens"); 
        }
        public bool HasSoulOotComposerBros()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_composer_bros_oot") || Has("soul_of_composer_bros"); 
        }
        public bool HasSoulMmComposerBros()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_composer_bros_mm") || Has("soul_of_composer_bros"); 
        }
        public bool HasSoulOotCuccoLady()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_cucco_lady_oot") || Has("soul_of_cucco_lady_anju"); 
        }
        public bool HasSoulMmAnju()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_anju_mm") || Has("soul_of_cucco_lady_anju"); 
        }
        public bool HasSoulOotDampe()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_dampe_oot") || Has("soul_of_dampe"); 
        }
        public bool HasSoulMmDampe()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_dampe_mm") || Has("soul_of_dampe"); 
        }
        public bool HasSoulOotDarunia()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_darunia_oot") || Has("soul_of_darunia"); 
        }
        public bool HasSoulMmDekuButler()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_deku_butler_mm") || Has("soul_of_deku_butler"); 
        }
        public bool HasSoulMmDekuKing()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_deku_king_mm") || Has("soul_of_deku_king"); 
        }
        public bool HasSoulMmDekuPrincess()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_deku_princess_mm") || Has("soul_of_deku_princess"); 
        }
        public bool HasSoulOotDogLady()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_dog_lady_oot") || Has("soul_of_dog_lady"); 
        }
        public bool HasSoulMmDogLady()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_dog_lady_mm") || Has("soul_of_dog_lady"); 
        }
        public bool HasSoulOotFishingPondOwner()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_fishing_pond_owner_oot") || Has("soul_of_fishing_pond_trading_post_owner"); 
        }
        public bool HasSoulMmTradingPostOwner()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_trading_post_owner_mm") || Has("soul_of_fishing_pond_trading_post_owner"); 
        }
        public bool HasSoulMmGoronElder()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_goron_elder_mm") || Has("soul_of_goron_elder"); 
        }
        public bool HasSoulOotGorons()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_goron_oot") || Has("soul_of_gorons"); 
        }
        public bool HasSoulMmGorons()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_gorons_mm") || Has("soul_of_gorons"); 
        }
        public bool HasSoulOotGoronShopkeeper()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_goron_shopkeeper_oot") || Has("soul_of_goron_shopkeeper"); 
        }
        public bool HasSoulMmGoronShopkeeper()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_goron_shopkeeper_mm") || Has("soul_of_goron_shopkeeper"); 
        }
        public bool HasSoulOotGraveyardKid()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_graveyard_kid_oot") || Has("soul_of_graveyard_kid_bombers"); 
        }
        public bool HasSoulMmBombers()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_bombers_mm") || Has("soul_of_graveyard_kid_bombers"); 
        }
        public bool HasSoulOotGuruGuru()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_guru_guru_oot") || Has("soul_of_guru_guru"); 
        }
        public bool HasSoulMmGuruGuru()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_guru_guru_mm") || Has("soul_of_guru_guru"); 
        }
        public bool HasSoulOotHoneyAndDarling()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_honey_and_darling_oot") || Has("soul_of_honey_and_darling"); 
        }
        public bool HasSoulMmHoneyAndDarling()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_honey_and_darling_mm") || Has("soul_of_honey_and_darling"); 
        }
        public bool HasSoulOotHylianGuard()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_hylian_guard_oot") || Has("soul_of_hylian_guard"); 
        }
        public bool HasSoulOotIngo()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_ingo_oot") || Has("soul_of_ingo_gorman_and_bros"); 
        }
        public bool HasSoulMmGormanBros()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_gorman_and_bros_mm") || Has("soul_of_ingo_gorman_and_bros"); 
        }
        public bool HasSoulMmKafei()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_kafei_mm") || Has("soul_of_kafei"); 
        }
        public bool HasSoulMmKeaton()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_keaton_mm") || Has("soul_of_keaton"); 
        }
        public bool HasSoulOotKingZora()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_king_zora_oot") || Has("soul_of_king_zora"); 
        }
        public bool HasSoulOotKokiri()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_kokiri_oot") || Has("soul_of_kokiri"); 
        }
        public bool HasSoulOotKokiriShopkeeper()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_kokiri_shopkeeper_oot") || Has("soul_of_kokiri_shopkeeper"); 
        }
        public bool HasSoulMmKoumeAndKotake()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_koume_and_kotake_mm") || Has("soul_of_koume_and_kotake"); 
        }
        public bool HasSoulMmMadameAroma()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_madame_aroma_mm") || Has("soul_of_madame_aroma"); 
        }
        public bool HasSoulOotMalon()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_malon_oot") || Has("soul_of_malon_romani_cremia"); 
        }
        public bool HasSoulMmRomaniCremia()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_romani_cremia_mm") || Has("soul_of_malon_romani_cremia"); 
        }
        public bool HasSoulMmMayorDotour()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_mayor_dotour_mm") || Has("soul_of_mayor_dotour"); 
        }
        public bool HasSoulOotMedigoron()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_medigoron_oot") || Has("soul_of_medigoron_keg_trial_goron"); 
        }
        public bool HasSoulMmKegTrialGoron()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_keg_trial_goron_mm") || Has("soul_of_medigoron_keg_trial_goron"); 
        }
        public bool HasSoulOotMido()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_mido_oot") || Has("soul_of_mido"); 
        }
        public bool HasSoulMmMoonChildren()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_moon_children_oot") || Has("soul_of_moon_children"); 
        }
        public bool HasSoulOotOldHag()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_old_hag_oot") || Has("soul_of_old_hag_anjus_grandmother"); 
        }
        public bool HasSoulMmAnjuGrandmother()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_anjus_grandmother_mm") || Has("soul_of_old_hag_anjus_grandmother"); 
        }
        public bool HasSoulOotPatrollingGerudos()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_patrolling_gerudos_oot") || Has("soul_of_the_patrolling_thieves_and_their_chief"); 
        }
        public bool HasSoulMmPatrollingPirates()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_patrolling_pirates_and_their_chief_mm") || Has("soul_of_the_patrolling_thieves_and_their_chief"); 
        }
        public bool HasSoulMmPlaygroundScrubs()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_playground_scrubs_mm") || Has("soul_of_playground_scrubs"); 
        }
        public bool HasSoulOotPoeCollector()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_poe_collector_oot") || Has("soul_of_poe_collector_ghost_hut_owner"); 
        }
        public bool HasSoulMmGhostHutOwner()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_ghost_hut_owner_mm") || Has("soul_of_poe_collector_ghost_hut_owner"); 
        }
        public bool HasSoulOotPotionShopkeeper()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_potion_shopkeeper_oot") || Has("soul_of_potion_shopkeeper"); 
        }
        public bool HasSoulOotPunkKid()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_punk_kid_oot") || Has("soul_of_punk_kid_grog"); 
        }
        public bool HasSoulMmGrog()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_grog_mm") || Has("soul_of_punk_kid_grog"); 
        }
        public bool HasSoulOotRooftopMan()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_rooftop_man_oot") || Has("soul_of_rooftop_man_part_timer"); 
        }
        public bool HasSoulMmPartTimer()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_part_timer_mm") || Has("soul_of_rooftop_man_part_timer"); 
        }
        public bool HasSoulOotRuto()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_ruto_oot") || Has("soul_of_ruto_lulu"); 
        }
        public bool HasSoulMmLulu()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_lulu_mm") || Has("soul_of_ruto_lulu"); 
        }
        public bool HasSoulOotSaria()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_saria_oot") || Has("soul_of_saria");
        }
        public bool HasSoulOotScientist()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_scientist_oot") || Has("soul_of_the_scientist"); 
        }
        public bool HasSoulMmScientist()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_scientist_mm") || Has("soul_of_the_scientist"); 
        }
        public bool HasSoulOotSheik()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_sheik_oot") || Has("soul_of_sheik"); 
        }
        public bool HasSoulOotShootingGalleryOwner()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_shooting_galery_owner_oot") || Has("soul_of_shooting_gallery_town_archery_owner"); 
        }
        public bool HasSoulMmTownArcheryOwner()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_town_archery_owner_mm") || Has("soul_of_shooting_gallery_town_archery_owner"); 
        }
        public bool HasSoulOotTalon()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_talon_oot") || Has("soul_of_talon_mr_barten"); 
        }
        public bool HasSoulMmMrBarten()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_mr_barten_mm") || Has("soul_of_talon_mr_barten"); 
        }
        public bool HasSoulMmTingle()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_tingle_mm") || Has("soul_of_tingle"); 
        }
        public bool HasSoulMmToiletHand()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_toilet_hand_mm") || Has("soul_of_toilet_hand"); 
        }
        public bool HasSoulMmToto()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_toto_mm") || Has("soul_of_toto"); 
        }
        public bool HasSoulOotZelda()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_zelda_oot") || Has("soul_of_zelda"); 
        }
        public bool HasSoulMmZoraMusicians()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_zora_musicians_mm") || Has("soul_of_zora_musicians"); 
        }
        public bool HasSoulOotZoras()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_zora_oot") || Has("soul_of_zoras"); 
        }
        public bool HasSoulMmZoras()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_zoras_mm") || Has("soul_of_zoras"); 
        }
        public bool HasSoulOotZoraShopkeeper()
        {
            if (!_config.SoulsNpcOot && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_zora_shopkeeper_oot") || Has("soul_of_zora_shopkeeper"); 
        }
        public bool HasSoulMmZoraShopkeeper()
        {
            if (!_config.SoulsNpcMm && !_config.SharedSoulsNpc) return true;
            return Has("soul_of_the_zora_shopkeeper_mm") || Has("soul_of_zora_shopkeeper"); 
        }
        //Animal Souls
        public bool HasSoulOotButterflies()
        {
            if (!_config.SoulsAnimalOot && !_config.SharedSoulsAnimal) return true;
            return Has("soul_of_butterflies_oot") || Has("soul_of_butterflies"); 
        }
        public bool HasSoulMmButterflies()
        {
            if (!_config.SoulsAnimalMm && !_config.SharedSoulsAnimal) return true;
            return Has("soul_of_butterflies_mm") || Has("soul_of_butterflies"); 
        }
        public bool HasSoulOotCows()
        {
            if (!_config.SoulsAnimalOot && !_config.SharedSoulsAnimal) return true;
            return Has("soul_of_cows_oot") || Has("soul_of_cows"); 
        }
        public bool HasSoulMmCows()
        {
            if (!_config.SoulsAnimalMm && !_config.SharedSoulsAnimal) return true;
            return Has("soul_of_cows_mm") || Has("soul_of_cows"); 
        }
        public bool HasSoulOotCuccos()
        {
            if (!_config.SoulsAnimalOot && !_config.SharedSoulsAnimal) return true;
            return Has("soul_of_cuccos_oot") || Has("soul_of_cuccos"); 
        }
        public bool HasSoulMmCuccos()
        {
            if (!_config.SoulsAnimalMm && !_config.SharedSoulsAnimal) return true;
            return Has("soul_of_cuccos_mm") || Has("soul_of_cuccos"); 
        }
        public bool HasSoulOotDogs()
        {
            if (!_config.SoulsAnimalOot && !_config.SharedSoulsAnimal) return true;
            return Has("soul_of_dogs_oot") || Has("soul_of_dogs"); 
        }
        public bool HasSoulMmDogs()
        {
            if (!_config.SoulsAnimalMm && !_config.SharedSoulsAnimal) return true;
            return Has("soul_of_dogs_mm") || Has("soul_of_dogs"); 
        }
        //Misc Souls
        public bool HasSoulOotBusinessScrubs()
        {
            if (!_config.SoulsMiscOot && !_config.SharedSoulsMisc) return true;
            return Has("soul_of_business_scrubs_oot") || Has("soul_of_business_scrubs"); 
        }
        public bool HasSoulMmBusinessScrubs()
        {
            if (!_config.SoulsMiscMm && !_config.SharedSoulsMisc) return true;
            return Has("soul_of_business_scrubs_mm") || Has("soul_of_business_scrubs"); 
        }
        public bool HasSoulOotGoldSkulltulas()
        {
            if (!_config.SoulsMiscOot && !_config.SharedSoulsMisc) return true;
            return Has("soul_of_gold_skulltulas_oot") || Has("soul_of_gold_skulltulas"); 
        }
        public bool HasSoulMmGoldSkulltulas()
        {
            if (!_config.SoulsMiscMm && !_config.SharedSoulsMisc) return true;
            return Has("soul_of_gold_skulltulas_mm") || Has("soul_of_gold_skulltulas"); 
        }
        public bool IsChild()
        {
            string startingAge = _config.StartingAge?.ToLower() ?? "child";
            string ageChange = _config.AgeChange?.ToLower() ?? "none";
            bool pulledMasterSword = _isLocationFound("OOT|Temple of Time Master Sword");
            if (startingAge == "child")
            { return true; }
            bool canTimeTravelBack = false;
            if (ageChange != "none")
            {
                if (ageChange == "always")
                { canTimeTravelBack = HasOotOcarina() && CanPlayOotSongOfTime(); }
                else if (ageChange == "oot")
                { canTimeTravelBack = HasOotOcarinaOfTime() && CanPlayOotSongOfTime(); }
            }
            if (_config.TimeTravelSword)
            { canTimeTravelBack = canTimeTravelBack && HasMasterSword(); }
            return pulledMasterSword || canTimeTravelBack;
        }
        public bool IsAdult()
        {
            string startingAge = _config.StartingAge?.ToLower() ?? "adult";
            string ageChange = _config.AgeChange?.ToLower() ?? "none";
            bool pulledMasterSword = _isLocationFound("OOT|Temple of Time Master Sword");
            if (startingAge == "adult")
            { return true; }
            bool canTimeTravelBack = false;
            if (ageChange != "none")
            {
                if (ageChange == "always")
                { canTimeTravelBack = HasOotOcarina() && CanPlayOotSongOfTime(); }
                else if (ageChange == "oot")
                { canTimeTravelBack = HasOotOcarinaOfTime() && CanPlayOotSongOfTime(); }
            }
            if (_config.TimeTravelSword)
            { canTimeTravelBack = canTimeTravelBack && HasMasterSword(); }
            return pulledMasterSword || canTimeTravelBack;
        }
    }
}
