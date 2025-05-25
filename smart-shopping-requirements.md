# Smart Shopping App - Documento Requisiti e Funzionalità

## 1. Panoramica del Progetto

### 1.1 Descrizione
App mobile multipiattaforma (.NET MAUI) per la gestione intelligente della spesa domestica che tiene traccia dell'inventario casalingo e genera automaticamente liste della spesa basate sui consumi e sulle soglie minime.

### 1.2 Obiettivi Principali
- Eliminare lo spreco alimentare
- Automatizzare la creazione delle liste della spesa
- Semplificare la gestione dell'inventario domestico
- Ottimizzare i tempi di acquisto al supermercato

## 2. Requisiti Funzionali

### 2.1 Gestione Inventario
**RF-001: Aggiunta Prodotti**
- Scanner barcode per aggiungere prodotti rapidamente
- Integrazione con OpenFoodFacts per informazioni automatiche
- Inserimento manuale per prodotti non trovati
- Categorizzazione automatica dei prodotti

**RF-002: Modifica Quantità**
- Incremento/decremento tramite interfaccia
- Scanner per consumo rapido
- Modalità "Finito" per azzeramento immediato
- Inserimento quantità personalizzata

**RF-003: Gestione Scadenze**
- Inserimento date di scadenza
- Notifiche per prodotti in scadenza
- Ordinamento per data di scadenza
- Alert automatici 3 giorni prima della scadenza

### 2.2 Lista della Spesa Intelligente
**RF-004: Generazione Automatica**
- Aggiunta automatica quando quantità < soglia minima
- Suggerimenti basati su pattern di consumo
- Prodotti correlati ("Se finisce il latte, servono i cereali?")
- Stima costi basata su prezzi storici

**RF-005: Organizzazione Lista**
- Raggruppamento per categorie
- Ordinamento per corsie supermercato
- Priorità prodotti (urgente, normale, quando capita)
- Condivisione lista con altri utenti

### 2.3 Scanner Multifunzione
**RF-006: Modalità Scanner**
- **Modalità Aggiungi**: per nuovi acquisti
- **Modalità Consuma**: per rimuovere dall'inventario
- **Modalità Verifica**: per controllare quantità
- Switch rapido tra modalità

**RF-007: Riconoscimento Avanzato**
- Support per barcode EAN-13, UPC-A, Code-128
- Riconoscimento anche con fotocamera di bassa qualità
- Feedback audio/vibrazione per scan riuscito
- Cronologia scan per annullare operazioni

### 2.4 Integrazione OpenFoodFacts
**RF-008: Recupero Informazioni**
- Nome prodotto, marca, categoria
- Immagine del prodotto
- Informazioni nutrizionali
- Unità di misura standard

**RF-009: Gestione Prodotti Non Trovati**
- Form di inserimento manuale
- Contribuzione al database OpenFoodFacts
- Cache locale per prodotti personalizzati
- Suggerimenti basati su prodotti simili

### 2.5 Gestione Impostazioni e Configurazione
**RF-010: Configurazione API**
- Inserimento URL personalizzato OpenFoodFacts
- Configurazione endpoint API alternativi
- Test connessione e validazione URL
- Gestione token di autenticazione API

**RF-011: Esportazione Database**
- Export in formato JSON, CSV, XML
- Selezione dati da esportare (completo/parziale)
- Condivisione file via email/cloud
- Cronologia esportazioni con metadata
- Pulizia automatica esportazioni vecchie

**RF-012: Gestione Backup e Ripristino**
- Backup automatico su cloud
- Esportazione manuale database locale
- Ripristino da backup precedenti
- Validazione integrità dati post-ripristino

## 3. Requisiti Non Funzionali

### 3.1 Performance
**RNF-001: Velocità Scanner**
- Riconoscimento barcode < 2 secondi
- Avvio camera < 1 secondo
- Risposta UI immediata (< 100ms)

**RNF-002: Sincronizzazione**
- Backup dati in cloud ogni 24h
- Sync multi-device in tempo reale
- Funzionamento offline completo

### 3.2 Usabilità
**RNF-003: Interfaccia**
- Design Material Design/iOS Human Interface
- Accessibilità per non vedenti (VoiceOver/TalkBack)
- Modalità dark/light
- Supporto tablet e smartphone

**RNF-004: Localizzazione**
- Supporto italiano completo
- Formati data/ora locali
- Valute locali per prezzi

### 3.3 Sicurezza e Privacy
**RNF-005: Dati Personali**
- Crittografia database locale
- Backup cloud crittografato
- Nessuna condivisione dati con terze parti
- Conformità GDPR

## 4. Architettura Tecnica

### 4.1 Stack Tecnologico
- **Framework**: .NET MAUI 9.0
- **Database**: SQLite con Entity Framework
- **Scanner**: ZXing.Net.Maui
- **Cloud**: Azure Mobile Apps (opzionale)
- **Pattern**: MVVM con CommunityToolkit.MVVM

### 4.2 Struttura Dati
```
Product
├── Barcode (string, PK)
├── Name (string)
├── Brand (string)
├── Category (string)
├── ImageUrl (string)
├── Unit (string)
├── NutritionalInfo (JSON)

InventoryItem
├── Id (int, PK)
├── Barcode (string, FK)
├── CurrentQuantity (decimal)
├── MinThreshold (decimal)
├── PurchaseDate (DateTime)
├── ExpiryDate (DateTime?)
├── LastUpdated (DateTime)
├── Location (string) // Frigo, Dispensa, etc.

ShoppingListItem
├── Id (int, PK)  
├── Barcode (string, FK)
├── QuantityNeeded (decimal)
├── Priority (enum)
├── EstimatedPrice (decimal)
├── IsCompleted (bool)
├── CreatedDate (DateTime)

AppSettings
├── Id (int, PK)
├── OpenFoodFactsApiUrl (string)
├── BackupApiUrl (string)
├── CustomApiEndpoint (string)
├── AutoBackupEnabled (bool)
├── NotificationsEnabled (bool)
├── Theme (enum: Light/Dark/Auto)
├── Language (string)
├── LastExportDate (DateTime?)
├── ExportFormat (enum: JSON/CSV/XML)

ExportData
├── ExportId (string, PK)
├── ExportDate (DateTime)
├── DataType (enum: Full/Inventory/ShoppingList)
├── Format (enum: JSON/CSV/XML)
├── FilePath (string)
├── FileSize (long)
```

### 4.3 Servizi Dettagliati

**SettingsService.cs**
```csharp
public class SettingsService
{
    public Task<AppSettings> GetSettingsAsync();
    public Task SaveSettingsAsync(AppSettings settings);
    public Task<bool> ValidateApiUrlAsync(string apiUrl);
    public Task ResetToDefaultsAsync();
    public string GetDefaultOpenFoodFactsUrl();
}
```

**ExportService.cs**
```csharp
public class ExportService
{
    public Task<string> ExportToJsonAsync(ExportType type);
    public Task<string> ExportToCsvAsync(ExportType type);
    public Task<string> ExportToXmlAsync(ExportType type);
    public Task<bool> ShareExportAsync(string filePath);
    public Task<List<ExportData>> GetExportHistoryAsync();
    public Task CleanupOldExportsAsync(int daysToKeep);
}
```

**ApiConfigurationService.cs**
```csharp
public class ApiConfigurationService
{
    public Task<bool> TestApiConnectionAsync(string apiUrl);
    public Task<ApiInfo> GetApiInfoAsync(string apiUrl);
    public string ValidateApiUrl(string url);
    public Task<List<string>> GetAvailableEndpointsAsync();
}
```

## 5. Interfaccia Utente

### 5.1 Navigazione Principale
- **Tab Bar Navigation**:
  - Inventario (icona casa)
  - Lista Spesa (icona carrello)
  - Scanner (icona camera)
  - Impostazioni (icona ingranaggio)

### 5.2 Schermate Dettagliate

**Schermata Inventario**
- Lista prodotti con quantità
- Filtri per categoria/scadenza
- Search bar per ricerca rapida
- Floating Action Button per scanner
- Swipe actions: modifica/elimina

**Schermata Lista Spesa**
- Prodotti raggruppati per categoria
- Checkbox per completamento
- Stima costo totale
- Pulsante "Condividi lista"
- Modalità "Shopping" ottimizzata

**Schermata Scanner**
- Viewfinder a schermo intero
- Overlay con istruzioni
- Bottoni modalità in basso
- Cronologia scan recenti
- Toggle flash/front camera

**Schermata Impostazioni**
- **Sezione API Configuration**:
  - Campo URL OpenFoodFacts API
  - Campo URL API personalizzata
  - Test connessione API
  - Reset alle impostazioni default
- **Sezione Esportazione**:
  - Formato export (JSON/CSV/XML)
  - Tipo dati (Completo/Solo Inventario/Solo Lista)
  - Pulsante "Esporta Database"
  - Cronologia esportazioni
  - Condivisione file esportati
- **Sezione Generale**:
  - Tema app (Chiaro/Scuro/Auto)
  - Lingua interfaccia
  - Notifiche push
  - Backup automatico
- **Sezione Avanzate**:
  - Cancella cache
  - Reset database
  - Diagnostica e log
  - Informazioni app

### 5.3 Interazioni e Gesture
- **Pull to refresh** per aggiornare liste
- **Swipe left** per azioni rapide
- **Long press** per selezione multipla
- **Shake** per accesso rapido scanner
- **Voice commands** per aggiunta vocale

## 6. Funzionalità Avanzate

### 6.1 Intelligenza Artificiale
**ML-001: Predizioni Consumo**
- Algoritmo per prevedere quando finiranno i prodotti
- Suggerimenti proattivi per acquisti
- Ottimizzazione soglie minime
- Pattern recognition per abitudini

**ML-002: Riconoscimento Scontrini**
- OCR per digitalizzare scontrini
- Aggiornamento automatico inventario
- Tracking prezzi e offerte
- Analisi spese per categoria

### 6.2 Integrazione Sociale
**SOC-001: Condivisione Famiglia**
- Sincronizzazione inventario familiare
- Notifiche push per lista spesa
- Assegnazione compiti ("Chi va a fare la spesa?")
- Chat interna per coordinamento

### 6.3 Gamification
**GAM-001: Sistema Punti**
- Punti per aggiornamenti inventario
- Badge per obiettivi (zero sprechi, etc.)
- Classifiche famiglia/amici
- Ricompense per comportamenti virtuosi

## 7. Integrazioni Esterne

### 7.1 API e Servizi
- **OpenFoodFacts API**: database prodotti
- **Servizi Cloud**: backup e sync
- **Notifiche Push**: Firebase/APNS
- **Mappe**: per localizzare supermercati

### 7.2 Piattaforme Specifiche
**Android**
- Widget homescreen
- Shortcuts dinamici
- Android Auto integration
- Wear OS companion

**iOS**  
- Siri Shortcuts
- Widgets iOS 14+
- Apple Watch app
- Handoff continuity

## 8. Piano di Sviluppo

### 8.1 Fase 1 - MVP (8 settimane)
- Gestione inventario base
- Scanner barcode semplice  
- Lista spesa manuale
- Database SQLite locale

### 8.2 Fase 2 - Intelligenza (6 settimane)
- Integrazione OpenFoodFacts
- Generazione automatica lista
- Gestione scadenze
- Modalità scanner multiple

### 8.3 Fase 3 - Cloud e Social (4 settimane)
- Backup cloud
- Condivisione liste
- Notifiche push
- Sincronizzazione multi-device

### 8.4 Fase 4 - AI e Avanzate (6 settimane)
- Machine Learning predizioni
- OCR scontrini
- Gamification
- Integrazione piattaforme specifiche

## 9. Metriche di Successo

### 9.1 KPI Funzionali
- Riduzione spreco alimentare: -30%
- Tempo creazione lista spesa: -80%
- Precisione inventario: >95%
- Completezza database prodotti: >90%

### 9.2 KPI Tecnici
- Crash rate: <0.1%
- Tempo caricamento app: <3s
- Successo rate scan: >98%
- Soddisfazione utente: >4.5/5

## 10. Rischi e Mitigazioni

### 10.1 Rischi Tecnici
- **Qualità camera scanner**: Test su dispositivi low-end
- **Accuratezza OpenFoodFacts**: Database fallback locale
- **Performance database**: Ottimizzazione query e indici
- **Connettività**: Modalità offline robusta

### 10.2 Rischi UX
- **Complessità interfaccia**: User testing iterativo
- **Learning curve**: Onboarding guidato
- **Abbandono app**: Gamification e notifiche intelligenti

## 11. Conclusioni

L'app Smart Shopping rappresenta una soluzione completa per la gestione domestica intelligente, combinando tecnologie moderne come ML, scanner avanzati e cloud sync per offrire un'esperienza utente superiore nella gestione quotidiana della spesa e dell'inventario casalingo.