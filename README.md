# 📅 Generátor HTML kalendářů

Moderní webová aplikace pro generování přizpůsobitelných HTML kalendářů s českými svátky, jmeninami a prázdninami.

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue)
![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WebAssembly-blueviolet)
![License](https://img.shields.io/badge/license-MIT-green)

## ✨ Funkce

### 🎨 Generování kalendářů
- **Přizpůsobitelné šablony**: Použijte výchozí šablonu nebo nahrajte vlastní HTML šablonu
- **Česká lokalizace**: Všechna data, názvy měsíců a svátky v češtině

### 🖼️ Správa obrázků
- **Chytré oříznutí obrázku**: Vestavěný Cropper.js s pevným poměrem stran 4,5:1
- **Více způsobů zadání**: Nahrajte soubor nebo zadejte URL
- **Automatická optimalizace**: Obrázky automaticky změněny na 1800×400px
- **Náhled a úprava**: Živý náhled s volitelným editorem ořezu
- **Sezónní výchozí obrázky**: Automatické sezónně vhodné obrázky z Unsplash

### 📆 Data o svátcích
- **Státní svátky**: České státní svátky
- **Školní prázdniny**: Regionální prázdniny podle okresů
- **Jmeniny**: České jmeniny se statistikami
- **Významné dny**: Památné a významné dny
- **Mezinárodní dny**: Celosvětové připomínkové dny

### 🚀 UX
- **Čisté rozhraní**: Jednoduché UI
- **Responzivní design**: Funguje na počítači i mobilních zařízeních
- **Náhled v novém okně**: Vygenerovaný kalendář se otevře v nové záložce prohlížeče
- **Validace v reálném čase**: Validace formuláře s užitečnými chybovými hláškami
- **Stažení šablony**: Stáhněte výchozí šablonu pro přizpůsobení

## 🏗️ Architektura

Projekt se skládá ze tří hlavních komponent:

```
CalendarApp/
├── CalendarAppCore/          # Základní knihovna (.NET Standard)
│   ├── KurzyApiClient.cs     # API klient pro stahování dat o svátcích
│   ├── MonthCalendar.cs      # Hlavní logika generování kalendáře
│   └── MonthCalendarOptions.cs # Konfigurační možnosti
│
├── CalendarApp.Api/          # ASP.NET Core Web API (Backend)
│   ├── Controllers/
│   │   └── CalendarController.cs # API endpointy
│   └── Program.cs            # Konfigurace API a CORS
│
└── CalendarApp.Web/          # Blazor WebAssembly (Frontend)
    ├── Pages/
    │   └── Home.razor        # Hlavní UI komponenta
    └── wwwroot/
        └── index.html        # JavaScript interop (Cropper.js)
```

### Technologický stack

**Backend:**
- .NET 9.0
- ASP.NET Core Web API
- HtmlAgilityPack (parsování HTML)

**Frontend:**
- Blazor WebAssembly
- Bootstrap 5
- Cropper.js (ořezávání obrázků)

**Zdroj dat:**
- kurzy.cz API (data o českých svátcích)
- Lokální ukládání do cache pro výkon

## 🚀 Začínáme

### Požadavky

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) nebo novější
- Moderní webový prohlížeč (Chrome, Firefox, Edge)

### Instalace

1. **Klonování repozitáře**
   ```bash
   git clone https://github.com/zcloudcz/CalendarApp.git
   cd CalendarApp/CalendarApp
   ```

2. **Obnovení závislostí**
   ```bash
   dotnet restore
   ```

### Spuštění aplikace

Musíte spustit API backend i Blazor frontend současně.

#### Možnost 1: Dva terminály

**Terminál 1 - API Backend:**
```bash
cd CalendarApp.Api
dotnet run
```
API běží na: `https://localhost:7109`

**Terminál 2 - Blazor Frontend:**
```bash
cd CalendarApp.Web
dotnet run
```
Webová aplikace běží na: `https://localhost:7216`

#### Možnost 2: Visual Studio

1. Otevřete `CalendarApp.sln` ve Visual Studiu
2. Nastavte `CalendarApp.Api` a `CalendarApp.Web` jako spouštěcí projekty
3. Stiskněte F5 pro spuštění obou projektů současně

### První použití

1. Otevřete prohlížeč a přejděte na `https://localhost:7216`
2. Rozbalovací seznam regionů se automaticky naplní dostupnými regiony
3. Vyberte rok a měsíc
4. Volitelně nahrajte obrázek nebo použijte výchozí sezónní obrázek
5. Klikněte na "Generovat kalendář" pro vytvoření kalendáře
6. Kalendář se automaticky otevře v nové záložce prohlížeče

## 📖 Návod k použití

### Základní generování kalendáře

1. **Výběr data**
   - Zvolte rok (2000-2100)
   - Zvolte měsíc z rozbalovacího seznamu (Leden-Prosinec)

2. **Konfigurace rozložení**
   - **Výchozí**: Standardní sekvenční rozložení (1-31)
   - **Dva sloupce**: Rozložení vedle sebe (1-15 | 16-31)

3. **Výběr regionu** (volitelné)
   - Vyberte z rozbalovacího seznamu pro filtrování školních prázdnin
   - Ponechte "-- Nevybráno --" pro celorepublikové svátky

4. **Přidání obrázku**
   - **Možnost A**: Zadejte URL obrázku
   - **Možnost B**: Nahrajte soubor obrázku (max 10MB)
     - Náhled se zobrazí automaticky
     - Klikněte na "Upravit výřez" pro výběr konkrétní oblasti
     - Ořez na poměr stran 4,5:1 (1800×400px)
     - Klikněte na "Použít výřez" pro potvrzení

5. **Přizpůsobení šablony** (volitelné)
   - Stáhněte výchozí šablonu pomocí odkazu
   - Upravte HTML/CSS podle potřeby
   - Nahrajte vlastní soubor šablony NEBO vložte obsah přímo

6. **Generování**
   - Klikněte na tlačítko "Generovat kalendář"
   - Kalendář se otevře v novém okně prohlížeče
   - Uložte pomocí Ctrl+S nebo pravým tlačítkem → Uložit jako

### Workflow ořezávání obrázků

```
Nahrání obrázku → Náhled → Kliknutí "Upravit výřez" → Výběr oblasti → Použít výřez → Generovat
```

Ořezávání automaticky:
- Udržuje poměr stran 4,5:1
- Změní velikost přesně na 1800×400px
- Optimalizuje jako JPEG s vysokou kvalitou

## 🎨 Přizpůsobení šablony

### Struktura šablony

HTML šablona používá zástupné značky, které jsou během generování nahrazeny:

```html
<!-- Globální zástupné značky -->
@image-url        → URL/cesta k obrázku v záhlaví
@month-name       → Název měsíce v češtině (např. "Leden")
@year             → Rok (např. "2024")

<!-- Sekce smyčky dnů -->
@foreach-begin
  <div class="calendar-row @alt">
    @date           → Číslo dne (1-31)
    @date-name      → Název dne (např. "Pondělí")
    @date-note      → Jmeniny
    @date-holidays  → Státní svátky
    @date-sholidays → Školní prázdniny
    @date-specialday → Významné památné dny
    @date-interday  → Mezinárodní připomínkové dny
  </div>
@foreach-end
```

### Doporučené specifikace

- **Rozměry obrázku**: 1800×400px (panoramatický formát)
- **Poměr stran**: 4,5:1
- **Limit velikosti šablony**: 1MB
- **Limit velikosti obrázku**: 5MB (nahrané), 10MB (pro ořezávání)

## 🔌 API Endpointy

Základní URL: `https://localhost:7109/api/calendar`

### Generování kalendáře
```http
POST /generate
Content-Type: multipart/form-data

Parametry:
- Year (int): 2000-2100
- Month (int): 1-12
- RenderMode (int): 0=Výchozí, 1=DvaSloupce
- Region (string, volitelné): např. "Blansko", "Praha"
- ImageFile (file, volitelné): JPG/PNG/GIF, max 5MB
- ImageUrl (string, volitelné): Alternativa k ImageFile
- TemplateFile (file, volitelné): HTML šablona, max 1MB
- TemplateContent (string, volitelné): Alternativa k TemplateFile

Vrací: Stažení HTML souboru
```

### Získání regionů
```http
GET /regions

Vrací: JSON pole dostupných regionů
{
  "regions": ["Blansko", "Brno-město", "Praha", ...]
}
```

### Získání výchozí šablony
```http
GET /default-template

Vrací: JSON s výchozí HTML šablonou
{
  "template": "<html>..."
}
```

### Nahrání obrázku
```http
POST /upload-image
Content-Type: multipart/form-data

Parametry:
- file (file): JPG/PNG/GIF, max 5MB

Vrací: JSON s URL nahraného souboru
{
  "url": "/uploads/abc123.jpg"
}
```

## 🏗️ Podrobnosti struktury projektu

### CalendarAppCore

**Účel**: Znovupoužitelná logika generování kalendáře

**Klíčové třídy:**
- `KurzyApiClient`: Stahuje a ukládá data o svátcích z kurzy.cz
- `MonthCalendar`: Hlavní generátor kalendáře se šablonovým enginem
- `MonthCalendarOptions`: Konfigurační objekt
- `Nameday`, `Holiday`: Datové modely

**Funkce:**
- Ukládání do souborové cache (snižuje volání externího API)
- Podpora šablon ze souboru i z paměti
- Filtrování podle regionu pro školní prázdniny
- Vestavěná výchozí šablona

### CalendarApp.Api

**Účel**: RESTful API backend

**Klíčové funkce:**
- Zpracování nahrávání souborů s validací
- Konfigurace CORS pro Blazor WASM
- Nastavení české kultury pro správné formátování data
- Zpracování chyb s lokalizovanými hláškami
- Servírování statických souborů pro nahrané obrázky

**Konfigurace:**
- Běží na portech: 7109 (HTTPS), 5148 (HTTP)
- CORS povolené originy: localhost:7216, localhost:5193
- Adresář pro nahrávání: `wwwroot/uploads/`

### CalendarApp.Web

**Účel**: Blazor WebAssembly frontend

**Klíčové komponenty:**
- `Home.razor`: Hlavní UI s formulářem a ořezáváním obrázků
- JavaScript interop pro integraci Cropper.js
- Responzivní layout Bootstrap 5

**Klíčové funkce:**
- Zpracování obrázků na straně klienta (bez nahrávání na server do generování)
- Náhled v reálném čase s editorem ořezu
- Validace formuláře s datovými anotacemi
- Dynamické načítání regionů při inicializaci komponenty

## 🛠️ Vývoj

### Sestavení

```bash
# Sestavení celého solution
dotnet build

# Sestavení konkrétního projektu
cd CalendarApp.Api
dotnet build

cd CalendarApp.Web
dotnet build
```

### Testování

```bash
# Spuštění API
cd CalendarApp.Api
dotnet watch run

# Spuštění Web (v samostatném terminálu)
cd CalendarApp.Web
dotnet watch run
```

Příkaz `watch` umožňuje hot reload během vývoje.

### Komentáře v kódu

Celý kód je profesionálně okomentován v angličtině:
- XML dokumentační komentáře pro všechna veřejná API
- JSDoc komentáře pro JavaScriptové funkce
- Inline komentáře vysvětlující komplexní logiku
- Vhodné pro junior/mid-level vývojáře

## 📝 Konfigurace

### Konfigurace portů

Upravte `Properties/launchSettings.json` v každém projektu pro změnu portů.

**API (CalendarApp.Api):**
```json
"applicationUrl": "https://localhost:7109;http://localhost:5148"
```

**Web (CalendarApp.Web):**
```json
"applicationUrl": "https://localhost:7216;http://localhost:5193"
```

⚠️ **Důležité**: Pokud změníte porty, aktualizujte nastavení CORS v `CalendarApp.Api/Program.cs` a API URL v `CalendarApp.Web/Pages/Home.razor`.

### Limity velikosti souborů

Upravte v `CalendarController.cs`:
```csharp
// Limit nahrávání obrázků
maxAllowedSize: 5 * 1024 * 1024  // 5MB

// Limit nahrávání šablon
maxAllowedSize: 1024 * 1024      // 1MB
```

## 🐛 Řešení problémů

### CORS chyby

**Problém**: Konzole prohlížeče zobrazuje CORS chyby

**Řešení**:
1. Ujistěte se, že API běží na portu 7109
2. Ujistěte se, že Web běží na portu 7216
3. Zkontrolujte konfiguraci CORS v `CalendarApp.Api/Program.cs`

### Nahrávání obrázku selže

**Problém**: Chyba "Velikost souboru překračuje limit"

**Řešení**: Zkontrolujte velikost souboru (max 5MB). Ořízněte nebo komprimujte obrázek před nahráním.

### Ořezávání nefunguje

**Problém**: Tlačítko ořezu nezobrazuje ořezávač

**Řešení**:
1. Zkontrolujte konzoli prohlížeče na JavaScriptové chyby
2. Ujistěte se, že se Cropper.js správně načetl (zkontrolujte záložku Network)
3. Zkuste stránku obnovit

### Rozbité české znaky

**Problém**: České znaky se zobrazují jako nesmysly (�)

**Řešení**: Toto je opraveno v aktuální verzi správným zpracováním UTF-8. Pokud stále vidíte problémy:
1. Ujistěte se, že `Program.cs` nastavuje českou kulturu
2. Zkontrolujte, že šablona má `<meta charset="UTF-8">`

## 🤝 Přispívání

Příspěvky jsou vítány! Neváhejte odeslat Pull Request.

1. Forkněte repozitář
2. Vytvořte feature branch (`git checkout -b feature/NovaPeknaFunkce`)
3. Commitněte změny (`git commit -m 'Přidána nějaká nová pěkná funkce'`)
4. Pushněte do větve (`git push origin feature/NovaPeknaFunkce`)
5. Otevřete Pull Request

## 📄 Licence

Tento projekt je licencován pod licencí MIT - viz soubor LICENSE pro podrobnosti.

## 🙏 Poděkování

- **kurzy.cz** - za poskytování dat o českých svátcích a jmeninách
- **Unsplash** - za výchozí sezónní obrázky
- **Cropper.js** - za funkcionalitu ořezávání obrázků
- **Bootstrap** - za responzivní UI komponenty

## 📧 Kontakt

Pro dotazy nebo podporu prosím otevřete issue na GitHubu.

---

**Poznámka**: Tato aplikace je určena pro generování českých kalendářů s lokálními daty o svátcích. UI podporuje výběr českého jazyka a regionální školní prázdniny pro české regiony.
