-- ============================================================
-- NETTOYAGE DES DOUBLONS — Table Analyses
-- GestionLaboratoire — Poulina Groupe Holding
-- À exécuter dans SQL Server Management Studio
-- ============================================================

-- ── ÉTAPE 1 : VOIR LES DOUBLONS DÉTECTÉS ──────────────────
-- Normalisation : suppression de " - ", " du ", " de ", " des "
-- puis comparaison insensible à la casse.
-- Un doublon = même nom normalisé, ou l'un contient l'autre.
-- TOUJOURS lire ce résultat avant de passer à l'étape 2.

WITH Normalise AS (
    SELECT
        Id,
        Nom,
        LOWER(
            LTRIM(RTRIM(
                REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    Nom,
                ' - ', ' '), ' du ', ' '), ' de ', ' '), ' des ', ' '), '  ', ' ')
            ))
        ) AS NomNorm
    FROM Analyses
)
SELECT
    a1.Id   AS [ID à conserver],
    a1.Nom  AS [Nom original],
    a2.Id   AS [ID doublon],
    a2.Nom  AS [Nom doublon],
    a1.NomNorm AS [Nom normalisé commun]
FROM Normalise a1
JOIN Normalise a2 ON a2.Id > a1.Id
WHERE
    a1.NomNorm = a2.NomNorm                     -- noms identiques après normalisation
    OR a2.NomNorm LIKE a1.NomNorm + '%'         -- a2 commence par le nom de a1
    OR a1.NomNorm LIKE a2.NomNorm + '%'         -- a1 commence par le nom de a2
ORDER BY a1.Id;


-- ── ÉTAPE 2 : VÉRIFIER LES RÉFÉRENCES (FK) ────────────────
-- Vérifier qu'aucun Résultat ne pointe vers les IDs à supprimer.
-- Si COUNT > 0 : exécuter d'abord l'étape 2b (réassignation).

SELECT
    r.AnalyseId         AS [AnalyseId doublon],
    COUNT(*)            AS [Nb résultats liés],
    a_orig.Id           AS [AnalyseId original à utiliser],
    a_orig.Nom          AS [Nom original]
FROM Resultats r
JOIN Analyses a_doublon ON a_doublon.Id = r.AnalyseId
JOIN (
    -- Trouver l'ID original correspondant (le plus bas avec même nom normalisé)
    WITH Normalise AS (
        SELECT Id, LOWER(LTRIM(RTRIM(
            REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Nom,' - ',' '),' du ',' '),' de ',' '),' des ',' '),'  ',' ')
        ))) AS NomNorm FROM Analyses
    )
    SELECT n2.Id AS DoublonId, MIN(n1.Id) AS OriginalId
    FROM Normalise n1
    JOIN Normalise n2 ON n2.Id > n1.Id
        AND (n1.NomNorm = n2.NomNorm OR n2.NomNorm LIKE n1.NomNorm + '%' OR n1.NomNorm LIKE n2.NomNorm + '%')
    GROUP BY n2.Id
) mapping ON mapping.DoublonId = r.AnalyseId
JOIN Analyses a_orig ON a_orig.Id = mapping.OriginalId
GROUP BY r.AnalyseId, a_orig.Id, a_orig.Nom;


-- ── ÉTAPE 2b (SI NÉCESSAIRE) : RÉASSIGNER LES RÉSULTATS ───
-- Exécuter SEULEMENT si l'étape 2 retourne des lignes.
-- Met à jour les résultats pour pointer vers l'ID original.

/*
WITH Normalise AS (
    SELECT Id, LOWER(LTRIM(RTRIM(
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Nom,' - ',' '),' du ',' '),' de ',' '),' des ',' '),'  ',' ')
    ))) AS NomNorm FROM Analyses
),
Mapping AS (
    SELECT n2.Id AS DoublonId, MIN(n1.Id) AS OriginalId
    FROM Normalise n1
    JOIN Normalise n2 ON n2.Id > n1.Id
        AND (n1.NomNorm = n2.NomNorm OR n2.NomNorm LIKE n1.NomNorm + '%' OR n1.NomNorm LIKE n2.NomNorm + '%')
    GROUP BY n2.Id
)
UPDATE r
SET r.AnalyseId = m.OriginalId
FROM Resultats r
JOIN Mapping m ON m.DoublonId = r.AnalyseId;
*/


-- ── ÉTAPE 3 : SUPPRIMER LES DOUBLONS ──────────────────────
-- Supprime en priorité les IDs connus (27, 28, 29, 30)
-- puis tous les autres doublons détectés par la normalisation.
-- Décommenter et exécuter après validation des étapes 1 et 2.

/*
-- 3a. Suppression des doublons connus (IDs explicitement identifiés)
DELETE FROM Analyses
WHERE Id IN (27, 28, 29, 30);

-- 3b. Suppression de tout autre doublon détecté automatiquement
-- (garde l'ID le plus bas par groupe de noms similaires)
WITH Normalise AS (
    SELECT Id, LOWER(LTRIM(RTRIM(
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Nom,' - ',' '),' du ',' '),' de ',' '),' des ',' '),'  ',' ')
    ))) AS NomNorm FROM Analyses
),
Doublons AS (
    SELECT n2.Id AS DoublonId
    FROM Normalise n1
    JOIN Normalise n2 ON n2.Id > n1.Id
        AND (n1.NomNorm = n2.NomNorm OR n2.NomNorm LIKE n1.NomNorm + '%' OR n1.NomNorm LIKE n2.NomNorm + '%')
    -- Vérifier qu'aucun résultat ne référence ce doublon
    WHERE NOT EXISTS (SELECT 1 FROM Resultats r WHERE r.AnalyseId = n2.Id)
)
DELETE FROM Analyses
WHERE Id IN (SELECT DoublonId FROM Doublons);

-- 3c. Vérification post-suppression
SELECT Id, Nom FROM Analyses ORDER BY Id;
*/
