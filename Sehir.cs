using System;
using MultiHubApp.Services;

namespace MultiHubApp.Models
{

    public class Sehir
    {
        public string SehirAdi { get; set; } = string.Empty;

        public string BugunUrl => HavaDurumuServisi.HavaDurumuBugunUrl(HavaDurumuServisi.NormalizeCityName(SehirAdi));
        public string BesGunUrl => HavaDurumuServisi.HavaDurumu5GunUrl(HavaDurumuServisi.NormalizeCityName(SehirAdi));
    }
}