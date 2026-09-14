-- ═══════════════════════════════════════════════════════════
-- LovEat – Batch 14 Migration
-- Module 27: Analytics & Reports
-- Module 28: Fraud Prevention (customer-side reporting)
-- ═══════════════════════════════════════════════════════════

-- ── Already created in previous batches (reference only) ──
-- Table: FraudAlerts     → FraudHeatMap model
-- Table: HeatMapPoints   → FraudHeatMap model
-- Table: RecommendationLogs → AIAnalytics model
-- Table: AnalyticsEvents → AIAnalytics model

-- ── Module 27: Analytics Events table (if not exists) ─────
CREATE TABLE IF NOT EXISTS "AnalyticsEvents" (
    "Id"          INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
    "UserId"      INTEGER     NULL,
    "EventType"   TEXT        NOT NULL,
    "EntityType"  TEXT        NULL,
    "EntityId"    INTEGER     NULL,
    "MetaData"    TEXT        NULL,
    "City"        TEXT        NULL,
    "Platform"    TEXT        NULL,
    "CreatedAt"   TEXT        NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_AnalyticsEvents_UserId"    ON "AnalyticsEvents" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AnalyticsEvents_EventType" ON "AnalyticsEvents" ("EventType");
CREATE INDEX IF NOT EXISTS "IX_AnalyticsEvents_CreatedAt" ON "AnalyticsEvents" ("CreatedAt");

-- ── Module 27: Recommendation Logs table (if not exists) ──
CREATE TABLE IF NOT EXISTS "RecommendationLogs" (
    "Id"                  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "UserId"              INTEGER NOT NULL,
    "RecommendedChefIds"  TEXT    NULL,
    "RecommendedCuisines" TEXT    NULL,
    "AlgorithmVersion"    TEXT    NULL DEFAULT 'v1.0',
    "Reason"              TEXT    NULL,
    "ClickedChefId"       INTEGER NULL,
    "BookingMade"         INTEGER NOT NULL DEFAULT 0,
    "CreatedAt"           TEXT    NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_RecommendationLogs_UserId" ON "RecommendationLogs" ("UserId");

-- ── Module 28: FraudAlerts table (if not exists) ──────────
CREATE TABLE IF NOT EXISTS "FraudAlerts" (
    "Id"               INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "UserId"           INTEGER NULL,
    "AlertType"        TEXT    NOT NULL,
    "Description"      TEXT    NOT NULL,
    "Severity"         TEXT    NOT NULL DEFAULT 'Medium',
    "Status"           TEXT    NOT NULL DEFAULT 'Open',
    "Resolution"       TEXT    NULL,
    "ResolvedByAdminId" INTEGER NULL,
    "ResolvedAt"       TEXT    NULL,
    "MetaData"         TEXT    NULL,
    "CreatedAt"        TEXT    NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_FraudAlerts_UserId"   ON "FraudAlerts" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_FraudAlerts_Status"   ON "FraudAlerts" ("Status");
CREATE INDEX IF NOT EXISTS "IX_FraudAlerts_Severity" ON "FraudAlerts" ("Severity");

-- ── Module 28: HeatMapPoints table (if not exists) ────────
CREATE TABLE IF NOT EXISTS "HeatMapPoints" (
    "Id"         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Latitude"   REAL    NOT NULL,
    "Longitude"  REAL    NOT NULL,
    "City"       TEXT    NULL,
    "Area"       TEXT    NULL,
    "DataType"   TEXT    NOT NULL DEFAULT 'BookingDemand',
    "Count"      INTEGER NOT NULL DEFAULT 1,
    "Revenue"    TEXT    NOT NULL DEFAULT '0',
    "RecordedAt" TEXT    NOT NULL DEFAULT (datetime('now')),
    "Period"     TEXT    NOT NULL DEFAULT 'Daily'
);

CREATE INDEX IF NOT EXISTS "IX_HeatMapPoints_DataType"   ON "HeatMapPoints" ("DataType");
CREATE INDEX IF NOT EXISTS "IX_HeatMapPoints_RecordedAt" ON "HeatMapPoints" ("RecordedAt");

-- ── Seed: Sample fraud alerts for testing ─────────────────
INSERT OR IGNORE INTO "FraudAlerts" ("Id","UserId","AlertType","Description","Severity","Status","CreatedAt")
VALUES
(1, NULL, 'SuspiciousPayment', '3 failed payment attempts in 24 hours',     'High',     'Open',     datetime('now','-2 hours')),
(2, NULL, 'FakeReview',        '4 reviews submitted in a single day',        'High',     'Investigating', datetime('now','-5 hours')),
(3, NULL, 'AbnormalBooking',   '5 bookings cancelled within 7 days',         'Medium',   'Open',     datetime('now','-1 day')),
(4, NULL, 'FakeLocation',      'GPS location mismatch detected for chef',    'Critical', 'Open',     datetime('now','-1 day')),
(5, NULL, 'MultipleAccounts',  'Same phone number used on 2 accounts',       'Critical', 'Resolved', datetime('now','-2 days'));

-- ── Seed: Sample analytics events ─────────────────────────
INSERT OR IGNORE INTO "AnalyticsEvents" ("Id","EventType","EntityType","Platform","City","CreatedAt")
VALUES
(1, 'AppOpen',        NULL,          'Android', 'Coimbatore', datetime('now','-1 hour')),
(2, 'Search',         'ChefSearch',  'iOS',     'Chennai',    datetime('now','-2 hours')),
(3, 'ChefView',       'Chef',        'Android', 'Madurai',    datetime('now','-3 hours')),
(4, 'BookingStart',   'Booking',     'iOS',     'Coimbatore', datetime('now','-4 hours')),
(5, 'BookingComplete','Booking',     'Android', 'Trichy',     datetime('now','-5 hours'));
