-- Script: Tạo Fake Snapshot trước đó 10 ngày dựa trên snapshot 'test'
-- Có thể chạy trực tiếp trong pgAdmin, DBeaver hoặc psql

DO $$
DECLARE
    v_source RECORD;
    v_new_id UUID := gen_random_uuid();
    v_new_name TEXT := 'test-old'; -- Tên của fake snapshot
    v_days_ago INT := 10;          -- Số ngày lùi về quá khứ
    v_reduced_card_data JSONB;
BEGIN
    -- 1. Tìm snapshot gốc tên 'test'
    SELECT * INTO v_source FROM "CardSnapshots" WHERE "Name" = 'test' LIMIT 1;
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Không tìm thấy snapshot tên test!';
    END IF;

    -- 2. Xóa snapshot cũ nếu đã tồn tại cùng tên
    DELETE FROM "CardSnapshots" 
    WHERE "HoyolabAccountId" = v_source."HoyolabAccountId" AND "Name" = v_new_name;

    -- 3. Tạo CardData cho snapshot cũ (giảm bớt số trận và số trận thắng để tạo chênh lệch tích cực)
    SELECT jsonb_object_agg(
        key,
        jsonb_build_object(
            'UseCount', GREATEST(0, (value->>'UseCount')::int - CASE 
                WHEN key IN ('1101', '1102', '1103') THEN 20  -- Giảm 20 trận với 3 thẻ chính
                WHEN (value->>'UseCount')::int > 10 THEN 3 
                ELSE 0 
            END),
            'Proficiency', GREATEST(0, (value->>'Proficiency')::int - CASE 
                WHEN key IN ('1101', '1102', '1103') THEN 14  -- Giảm 14 trận thắng
                WHEN (value->>'Proficiency')::int > 5 THEN 2 
                ELSE 0 
            END)
        )
    )
    INTO v_reduced_card_data
    FROM jsonb_each(v_source."CardData");

    -- 4. Chèn bản lưu fake vào database
    INSERT INTO "CardSnapshots" (
        "Id",
        "HoyolabAccountId",
        "DiscordUserId",
        "Name",
        "SnapshotType",
        "CreatedAt",
        "TotalGames",
        "TotalWins",
        "CardData"
    ) VALUES (
        v_new_id,
        v_source."HoyolabAccountId",
        v_source."DiscordUserId",
        v_new_name,
        'UserCustom',
        v_source."CreatedAt" - (v_days_ago || ' days')::interval,
        v_source."TotalGames" - 40, -- Giảm 40 trận
        v_source."TotalWins" - 26,   -- Giảm 26 trận thắng
        v_reduced_card_data
    );

    RAISE NOTICE 'Đã tạo fake snapshot % thành công (thời gian: %)!', 
        v_new_name, 
        v_source."CreatedAt" - (v_days_ago || ' days')::interval;
END $$;
