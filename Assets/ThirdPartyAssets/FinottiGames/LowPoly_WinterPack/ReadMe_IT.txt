[ITA]
Versione: 1.0 Publisher: FinottiGames

Grazie per aver acquistato WinterPack! Questo pacchetto è stato progettato per essere leggero, modulare e performante, ideale per progetti Mobile, VR e PC. Tutti gli asset condividono un singolo materiale per garantire il massimo delle prestazioni (Batching ottimizzato).

📦 Contenuto del Pacchetto
Tutti gli asset sono disponibili in due varianti: Standard e Innevata.

3x Baite in Legno (Solo esterni, non visitabili internamente).

3x Pini Low Poly (Diverse forme e dimensioni).

1x Lampione Stradale (Inclusa versione con luce emissiva).

Environment Bonus: La scena Demo include asset ambientali (Terreno, Ghiacciai, Particle System per la neve) che potete utilizzare liberamente per arricchire i vostri sfondi.

🎨 Materiali e Texture
Il pacchetto utilizza un workflow altamente ottimizzato con un unico Materiale Atlas condiviso tra tutti gli oggetti. Texture incluse (Risoluzione consigliata 256px o superiore):

Albedo (BaseColor): La palette dei colori.

Attributes Map (Mask Map):

Canale R (Rosso): Metallic

Canale A (Alpha): Smoothness

Emission Map: Gestisce le luci delle finestre e dei lampioni.

Nota: Se possiedi altri pacchetti di FinottiGames, questo materiale è compatibile e intercambiabile.

⚙️ Guida all'Installazione (Render Pipelines)
Il progetto viene fornito di default con i materiali impostati per Built-in Render Pipeline. Se utilizzi URP o HDRP, segui questi passaggi per aggiornare i materiali.

1. Setup Iniziale
Assicurati di avere installato il pacchetto della pipeline desiderata (URP o HDRP) tramite il Package Manager (Window > Package Manager > Unity Registry).

2. Configurazione del Progetto (Se inizi da zero)
Nel pannello Project, crea il tuo Render Pipeline Asset (es: Create > Rendering > URP Asset).

Vai su Edit > Project Settings > Graphics.

Assegna il file appena creato nel campo Scriptable Render Pipeline Settings.

Nota: Se usi HDRP/URP e vedi tutto rosa, è normale. Procedi al punto 3.

3. Sostituzione dei Materiali
Per garantire la corretta visualizzazione, abbiamo incluso materiali specifici per ogni pipeline.

Vai nella cartella: FinottiGames/LowPoly_WinterPack/Materials-Shaders/

Apri la cartella corrispondente alla tua pipeline (es. URP o HDRP).

Seleziona i Prefab o gli oggetti nella scena e trascina il materiale corretto nello slot del Mesh Renderer.

Alternativa: Puoi semplicemente copiare i settaggi del materiale dalla cartella specifica e incollarli sul materiale principale se preferisci mantenere i collegamenti esistenti.

💡 Suggerimenti Visivi
Bloom: Per far "brillare" le finestre e i lampioni (Texture Emission), è necessario attivare l'effetto Bloom nel Global Volume della tua scena.

Neve: Il Particle System incluso nella demo è pensato per essere agganciato alla Main Camera per simulare una nevicata infinita ottimizzata.

📞 Supporto
Per qualsiasi problema, bug o richiesta, non esitare a contattarmi: info@finottigames.com


