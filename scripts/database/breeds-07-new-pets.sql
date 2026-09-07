USE [pastil_db]
GO
-- نژادها/گونه‌های دسته‌های جدید (خوکچه هندی=7، لاک‌پشت=8، خزنده=9، جوجه‌تیغی=10، شینچیلا=11، موش فانتزی=12)
-- پیش‌نیاز: اسکریپت add-pets-batch1.sql قبلاً اجرا شده باشه.

-- خوکچه هندی - لیست کامل نژادهای رسمی (ARBA/BRC)
INSERT INTO [dbo].[PetBreeds] ([PetId], [Label], [PictureId], [Priority], [Deleted], [Name], [Slug]) VALUES
(7, N'American Guinea Pig', NULL, 1, 0, N'خوکچه هندی آمریکایی', N'american-guinea-pig'),
(7, N'Abyssinian Guinea Pig', NULL, 2, 0, N'خوکچه هندی حبشی', N'abyssinian-guinea-pig'),
(7, N'Peruvian Guinea Pig', NULL, 3, 0, N'خوکچه هندی پرویی', N'peruvian-guinea-pig'),
(7, N'Silkie Guinea Pig', NULL, 4, 0, N'خوکچه هندی سیلکی', N'silkie-guinea-pig'),
(7, N'Texel Guinea Pig', NULL, 5, 0, N'خوکچه هندی تکسل', N'texel-guinea-pig'),
(7, N'Coronet Guinea Pig', NULL, 6, 0, N'خوکچه هندی کرونت', N'coronet-guinea-pig'),
(7, N'Skinny Pig', NULL, 7, 0, N'خوکچه هندی بی‌مو (اسکینی)', N'skinny-pig'),
(7, N'White Crested Guinea Pig', NULL, 8, 0, N'خوکچه هندی تاج‌سفید', N'white-crested-guinea-pig'),
(7, N'Rex Guinea Pig', NULL, 9, 0, N'خوکچه هندی رکس', N'rex-guinea-pig'),
(7, N'Teddy Guinea Pig', NULL, 10, 0, N'خوکچه هندی تدی', N'teddy-guinea-pig'),
(7, N'Himalayan Guinea Pig', NULL, 11, 0, N'خوکچه هندی هیمالین', N'himalayan-guinea-pig'),
(7, N'Merino Guinea Pig', NULL, 12, 0, N'خوکچه هندی مرینو', N'merino-guinea-pig'),
(7, N'Lunkarya Guinea Pig', NULL, 13, 0, N'خوکچه هندی لانکاریا', N'lunkarya-guinea-pig'),
(7, N'Other', NULL, 14, 0, N'سایر', N'other-guinea-pig');
GO

-- لاک‌پشت / تستودو
INSERT INTO [dbo].[PetBreeds] ([PetId], [Label], [PictureId], [Priority], [Deleted], [Name], [Slug]) VALUES
(8, N'Red-Eared Slider', NULL, 1, 0, N'لاک‌پشت گوش‌قرمز', N'red-eared-slider'),
(8, N'Yellow-Bellied Slider', NULL, 2, 0, N'لاک‌پشت شکم‌زرد', N'yellow-bellied-slider'),
(8, N'Painted Turtle', NULL, 3, 0, N'لاک‌پشت نقاشی', N'painted-turtle'),
(8, N'Map Turtle', NULL, 4, 0, N'لاک‌پشت نقشه‌ای', N'map-turtle'),
(8, N'Musk Turtle', NULL, 5, 0, N'لاک‌پشت مشکی (موسک)', N'musk-turtle'),
(8, N'Mud Turtle', NULL, 6, 0, N'لاک‌پشت گلی', N'mud-turtle'),
(8, N'Russian Tortoise', NULL, 7, 0, N'لاک‌پشت روسی', N'russian-tortoise'),
(8, N'Greek Tortoise', NULL, 8, 0, N'لاک‌پشت یونانی', N'greek-tortoise'),
(8, N'Sulcata Tortoise', NULL, 9, 0, N'لاک‌پشت سولکاتا (آفریقایی)', N'sulcata-tortoise'),
(8, N'Hermann s Tortoise', NULL, 10, 0, N'لاک‌پشت هرمان', N'hermanns-tortoise'),
(8, N'Box Turtle', NULL, 11, 0, N'لاک‌پشت جعبه‌ای', N'box-turtle'),
(8, N'Snapping Turtle', NULL, 12, 0, N'لاک‌پشت گازانبری', N'snapping-turtle'),
(8, N'Softshell Turtle', NULL, 13, 0, N'لاک‌پشت لاک‌نرم', N'softshell-turtle'),
(8, N'Reeve s Turtle', NULL, 14, 0, N'لاک‌پشت ریوز', N'reeves-turtle'),
(8, N'Indian Star Tortoise', NULL, 15, 0, N'لاک‌پشت ستاره‌ای هندی', N'indian-star-tortoise'),
(8, N'Leopard Tortoise', NULL, 16, 0, N'لاک‌پشت پلنگی', N'leopard-tortoise'),
(8, N'Other', NULL, 17, 0, N'سایر', N'other-turtle');
GO

-- خزنده (مارمولک/مار)
INSERT INTO [dbo].[PetBreeds] ([PetId], [Label], [PictureId], [Priority], [Deleted], [Name], [Slug]) VALUES
(9, N'Bearded Dragon', NULL, 1, 0, N'اژدهای ریش‌دار', N'bearded-dragon'),
(9, N'Leopard Gecko', NULL, 2, 0, N'گکو پلنگی', N'leopard-gecko'),
(9, N'Crested Gecko', NULL, 3, 0, N'گکو تاجدار', N'crested-gecko'),
(9, N'Blue-Tongue Skink', NULL, 4, 0, N'اسکینک زبان‌آبی', N'blue-tongue-skink'),
(9, N'Ball Python', NULL, 5, 0, N'پیتون توپی', N'ball-python'),
(9, N'Corn Snake', NULL, 6, 0, N'مار ذرت', N'corn-snake'),
(9, N'Green Iguana', NULL, 7, 0, N'ایگوانای سبز', N'green-iguana'),
(9, N'Veiled Chameleon', NULL, 8, 0, N'آفتاب‌پرست حجابی', N'veiled-chameleon'),
(9, N'Tegu', NULL, 9, 0, N'تگو', N'tegu'),
(9, N'Uromastyx', NULL, 10, 0, N'دم‌بزمجه (اورومستیکس)', N'uromastyx'),
(9, N'King Snake', NULL, 11, 0, N'مار پادشاهی', N'king-snake'),
(9, N'Milk Snake', NULL, 12, 0, N'مار شیری', N'milk-snake'),
(9, N'Boa Constrictor', NULL, 13, 0, N'بوآ کانستریکتور', N'boa-constrictor'),
(9, N'Chinese Water Dragon', NULL, 14, 0, N'اژدهای آبی چینی', N'chinese-water-dragon'),
(9, N'Green Anole', NULL, 15, 0, N'آنول سبز', N'green-anole'),
(9, N'House Gecko', NULL, 16, 0, N'گکوی خانگی', N'house-gecko'),
(9, N'Tokay Gecko', NULL, 17, 0, N'گکو توکای', N'tokay-gecko'),
(9, N'Savannah Monitor', NULL, 18, 0, N'مانیتور ساوانا', N'savannah-monitor'),
(9, N'Garter Snake', NULL, 19, 0, N'مار گارتر', N'garter-snake'),
(9, N'Ackie Monitor', NULL, 20, 0, N'مانیتور آکی', N'ackie-monitor'),
(9, N'Frilled Dragon', NULL, 21, 0, N'اژدهای یقه‌دار', N'frilled-dragon'),
(9, N'Gargoyle Gecko', NULL, 22, 0, N'گکو گارگویل', N'gargoyle-gecko'),
(9, N'Other', NULL, 23, 0, N'سایر', N'other-reptile');
GO

-- جوجه‌تیغی
INSERT INTO [dbo].[PetBreeds] ([PetId], [Label], [PictureId], [Priority], [Deleted], [Name], [Slug]) VALUES
(10, N'African Pygmy Hedgehog', NULL, 1, 0, N'جوجه‌تیغی پیگمی آفریقایی', N'african-pygmy-hedgehog'),
(10, N'Algerian Hedgehog', NULL, 2, 0, N'جوجه‌تیغی الجزایری', N'algerian-hedgehog'),
(10, N'Long-Eared Hedgehog', NULL, 3, 0, N'جوجه‌تیغی گوش‌دراز', N'long-eared-hedgehog'),
(10, N'Other', NULL, 4, 0, N'سایر', N'other-hedgehog');
GO

-- شینچیلا (بر اساس رنگ - چون نژاد رسمی جدا نداره)
INSERT INTO [dbo].[PetBreeds] ([PetId], [Label], [PictureId], [Priority], [Deleted], [Name], [Slug]) VALUES
(11, N'Standard Grey Chinchilla', NULL, 1, 0, N'شینچیلای خاکستری استاندارد', N'standard-grey-chinchilla'),
(11, N'White Chinchilla', NULL, 2, 0, N'شینچیلای سفید', N'white-chinchilla'),
(11, N'Beige Chinchilla', NULL, 3, 0, N'شینچیلای بژ', N'beige-chinchilla'),
(11, N'Black Velvet Chinchilla', NULL, 4, 0, N'شینچیلای مخمل‌مشکی', N'black-velvet-chinchilla'),
(11, N'Violet Chinchilla', NULL, 5, 0, N'شینچیلای بنفش', N'violet-chinchilla'),
(11, N'Sapphire Chinchilla', NULL, 6, 0, N'شینچیلای یاقوت‌کبود', N'sapphire-chinchilla'),
(11, N'Ebony Chinchilla', NULL, 7, 0, N'شینچیلای آبنوسی', N'ebony-chinchilla'),
(11, N'Tan Chinchilla', NULL, 8, 0, N'شینچیلای تن', N'tan-chinchilla'),
(11, N'Wilson White Chinchilla', NULL, 9, 0, N'شینچیلای ویلسون‌وایت', N'wilson-white-chinchilla'),
(11, N'Homo Beige Chinchilla', NULL, 10, 0, N'شینچیلای هومو بژ', N'homo-beige-chinchilla'),
(11, N'Other', NULL, 11, 0, N'سایر', N'other-chinchilla');
GO

-- موش فانتزی (بر اساس نوع پوشش/رنگ - چون نژاد رسمی جدا نداره)
INSERT INTO [dbo].[PetBreeds] ([PetId], [Label], [PictureId], [Priority], [Deleted], [Name], [Slug]) VALUES
(12, N'Standard Fancy Mouse', NULL, 1, 0, N'موش فانتزی استاندارد', N'standard-fancy-mouse'),
(12, N'Satin Mouse', NULL, 2, 0, N'موش ساتن', N'satin-mouse'),
(12, N'Long-Haired Mouse', NULL, 3, 0, N'موش موبلند', N'long-haired-mouse'),
(12, N'Rex Mouse', NULL, 4, 0, N'موش رکس', N'rex-mouse'),
(12, N'Himalayan Mouse', NULL, 5, 0, N'موش هیمالین', N'himalayan-mouse'),
(12, N'Siamese Mouse', NULL, 6, 0, N'موش سیامی', N'siamese-mouse'),
(12, N'Dutch Mouse', NULL, 7, 0, N'موش داچ', N'dutch-mouse'),
(12, N'Fuzzy Mouse', NULL, 8, 0, N'موش فازی (کم‌مو)', N'fuzzy-mouse'),
(12, N'Tan Mouse', NULL, 9, 0, N'موش تن', N'tan-mouse'),
(12, N'Merle Mouse', NULL, 10, 0, N'موش مرل', N'merle-mouse'),
(12, N'Other', NULL, 11, 0, N'سایر', N'other-mouse');
GO
