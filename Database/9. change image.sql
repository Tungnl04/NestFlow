UPDATE PropertyImages
SET image_url = 
    CASE display_order
        WHEN 0 THEN '/uploads/properties/nha-tro-minh-anh/minhanh1.jpg'
        WHEN 1 THEN '/uploads/properties/nha-tro-minh-anh/minhanh2.jpg'
        WHEN 2 THEN '/uploads/properties/nha-tro-minh-anh/minhanh3.jpg'
        WHEN 3 THEN '/uploads/properties/nha-tro-minh-anh/minhanh4.jpg'
        WHEN 4 THEN '/uploads/properties/nha-tro-minh-anh/minhanh5.jpg'
    END
WHERE property_id = 128
