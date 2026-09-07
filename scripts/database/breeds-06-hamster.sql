USE [pastil_db]
GO
-- همستر - ۶ گونه‌ی اصلی از قبل ثبت شده؛ این‌ها گونه/نوع پوششی متمایز دیگه‌ای هستن که در بازار جدا فروخته می‌شن

INSERT INTO [dbo].[PetBreeds] ([PetId], [Label], [PictureId], [Priority], [Deleted], [Name], [Slug]) VALUES
(6, N'Teddy Bear Hamster', NULL, 7, 0, N'همستر خرس‌عروسکی (سوری موبلند)', N'teddy-bear-hamster'),
(6, N'Black Bear Hamster', NULL, 8, 0, N'همستر خرس‌سیاه (سوری مشکی)', N'black-bear-hamster'),
(6, N'Panda Bear Hamster', NULL, 9, 0, N'همستر پاندا', N'panda-bear-hamster'),
(6, N'European Hamster', NULL, 10, 0, N'همستر اروپایی', N'european-hamster');
GO
