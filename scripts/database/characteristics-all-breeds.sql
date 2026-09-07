USE [pastil_db]
GO
-- خصوصیات کامل همه‌ی نژادها/گونه‌های موجود - یک اسکریپت واحد

-- خصوصیات نژادهای سگ (۵۸ نژاد اصلی/محبوب موجود). PetBreedId با ساب‌کوئری روی Label
-- پیدا می‌شه، پس به Id واقعی (که IDENTITY خودکار تخصیص داده) وابسته نیست.

INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'German Shepherd'), N'وفادار و محافظ خانواده', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'German Shepherd'), N'باهوش و به‌سرعت آموزش‌پذیر', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'German Shepherd'), N'انرژی بالا و نیازمند فعالیت روزانه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'German Shepherd'), N'مستعد دیسپلازی لگن', 0, 3, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'German Shepherd'), N'ریزش موی نسبتاً زیاد', 0, 4, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Labrador Retriever'), N'بسیار دوستانه و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Labrador Retriever'), N'مناسب خانواده و بچه‌ها', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Labrador Retriever'), N'اشتهای زیاد و مستعد چاقی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Labrador Retriever'), N'نیاز به ورزش روزانه', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Golden Retriever'), N'آرام و صبور با کودکان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Golden Retriever'), N'بسیار وفادار و مهربان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Golden Retriever'), N'ریزش موی فصلی سنگین', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Golden Retriever'), N'مستعد بیماری‌های مفصلی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Poodle'), N'بسیار باهوش و آموزش‌پذیر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Poodle'), N'ریزش موی کم و مناسب حساسیت', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Poodle'), N'نیاز به آرایش و مراقبت مرتب از مو', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Poodle'), N'حساس به تنهایی طولانی‌مدت', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Rottweiler'), N'محافظ قوی و شجاع', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rottweiler'), N'وفادار به صاحب خود', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rottweiler'), N'نیاز جدی به تربیت و اجتماعی‌سازی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rottweiler'), N'می‌تواند نسبت به غریبه‌ها بدبین باشد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Doberman Pinscher'), N'هوشمند و بسیار وفادار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Doberman Pinscher'), N'محافظ عالی برای خانه', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Doberman Pinscher'), N'انرژی زیاد و نیاز به فعالیت مداوم', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Doberman Pinscher'), N'حساس به سرما به‌خاطر موی کوتاه', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Siberian Husky'), N'اجتماعی و دوستانه با انسان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siberian Husky'), N'مقاوم در برابر سرما', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siberian Husky'), N'سرسخت و سخت‌آموزش', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siberian Husky'), N'تمایل زیاد به فرار و پرسه‌زنی', 0, 3, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siberian Husky'), N'ریزش موی بسیار زیاد', 0, 4, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Alaskan Malamute'), N'قوی و بادوام برای کار سنگین', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Alaskan Malamute'), N'مهربان با اعضای خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Alaskan Malamute'), N'نیاز به فضای زیاد و فعالیت بالا', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Alaskan Malamute'), N'ریزش موی فصلی سنگین', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Samoyed'), N'بسیار دوستانه و شاد', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Samoyed'), N'مهربان با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Samoyed'), N'نیاز به شانه‌کشیدن مرتب موی سفید بلند', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Samoyed'), N'پارس زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Akita Inu'), N'وفادار و مصمم', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Akita Inu'), N'محافظ خوب خانه', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Akita Inu'), N'مستقل و گاهی لجباز', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Akita Inu'), N'ممکن است با سگ‌های دیگر تهاجمی باشد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Shiba Inu'), N'باهوش و تمیز', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shiba Inu'), N'اندازه کوچک مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shiba Inu'), N'مستقل و کله‌شق در آموزش', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shiba Inu'), N'تمایل بالا به فرار', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Belgian Malinois'), N'بسیار باهوش و کارآمد', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Belgian Malinois'), N'وفادار و آموزش‌پذیر عالی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Belgian Malinois'), N'انرژی فوق‌العاده بالا', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Belgian Malinois'), N'برای صاحب بی‌تجربه مناسب نیست', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Border Collie'), N'باهوش‌ترین نژاد سگ جهان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Border Collie'), N'فوق‌العاده آموزش‌پذیر', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Border Collie'), N'نیاز شدید به تحریک ذهنی و فعالیت', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Border Collie'), N'گرایش به گله کردن اعضای خانواده', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Australian Shepherd'), N'باهوش و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Australian Shepherd'), N'وفادار به خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Australian Shepherd'), N'نیاز به فعالیت بدنی و ذهنی زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Australian Shepherd'), N'مستعد مشکلات چشمی ژنتیکی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Beagle'), N'دوستانه و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Beagle'), N'حس بویایی فوق‌العاده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Beagle'), N'زوزه و پارس زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Beagle'), N'تمایل به دنبال‌کردن بو و فرار', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Basset Hound'), N'آرام و صبور', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Basset Hound'), N'مهربان با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Basset Hound'), N'مستعد چاقی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Basset Hound'), N'مستعد مشکلات کمر به‌خاطر بدن کشیده', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Bloodhound'), N'حس بویایی بی‌نظیر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bloodhound'), N'ملایم و صبور', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bloodhound'), N'آبریزش دهان زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bloodhound'), N'سرسخت در پیروی از دستورات', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'English Bulldog'), N'آرام و بی‌سروصدا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Bulldog'), N'مهربان با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Bulldog'), N'مشکلات تنفسی به‌خاطر پوزه کوتاه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Bulldog'), N'حساس به گرما', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'French Bulldog'), N'شوخ و بامزه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'French Bulldog'), N'مناسب آپارتمان و کم‌تحرک نیست به‌شدت', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'French Bulldog'), N'مشکلات تنفسی پوزه‌کوتاه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'French Bulldog'), N'ناتوانی در شنای طبیعی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Boxer'), N'پرانرژی و بازیگوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Boxer'), N'وفادار و محافظ خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Boxer'), N'مستعد بیماری‌های قلبی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Boxer'), N'حساس به گرمای زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Great Dane'), N'مهربان و ملایم علی‌رغم جثه بزرگ', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Great Dane'), N'صبور با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Great Dane'), N'عمر نسبتاً کوتاه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Great Dane'), N'هزینه نگهداری و غذای زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Saint Bernard'), N'مهربان و آرام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Saint Bernard'), N'صبور و خوب با بچه‌ها', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Saint Bernard'), N'آبریزش دهان زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Saint Bernard'), N'نیاز به فضای زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Cane Corso'), N'محافظ قدرتمند و باوقار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cane Corso'), N'وفادار به خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cane Corso'), N'نیاز به تربیت‌کننده باتجربه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cane Corso'), N'ممکن است نسبت به غریبه‌ها بدبین باشد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'English Mastiff'), N'آرام و باوقار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Mastiff'), N'مهربان با خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Mastiff'), N'عمر کوتاه به‌خاطر جثه غول‌پیکر', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Mastiff'), N'هزینه غذا و مراقبت بالا', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Tibetan Mastiff'), N'محافظ بسیار قدرتمند', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tibetan Mastiff'), N'مستقل و باهوش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tibetan Mastiff'), N'پارس شبانه زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tibetan Mastiff'), N'سرسخت و سخت‌کنترل', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Dalmatian'), N'پرانرژی و بازیگوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dalmatian'), N'وفادار به خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dalmatian'), N'نیاز شدید به ورزش روزانه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dalmatian'), N'مستعد ناشنوایی مادرزادی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Chihuahua'), N'اندازه بسیار کوچک، مناسب آپارتمان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chihuahua'), N'وفادار به یک نفر خاص', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chihuahua'), N'پارس زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chihuahua'), N'حساس به سرما', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Pomeranian'), N'شاداب و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pomeranian'), N'اندازه کوچک و ظاهر جذاب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pomeranian'), N'پارس زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pomeranian'), N'نیاز به مراقبت مرتب از موی پرپشت', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Pug'), N'شوخ و بامزه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pug'), N'مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pug'), N'مشکلات تنفسی پوزه‌کوتاه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pug'), N'مستعد چاقی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Yorkshire Terrier'), N'شجاع علی‌رغم اندازه کوچک', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Yorkshire Terrier'), N'ریزش موی کم', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Yorkshire Terrier'), N'پارس زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Yorkshire Terrier'), N'شکننده و نیازمند مراقبت ویژه', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Shih Tzu'), N'مهربان و آرام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shih Tzu'), N'مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shih Tzu'), N'مشکلات تنفسی پوزه‌کوتاه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shih Tzu'), N'نیاز به آرایش مرتب مو', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Maltese'), N'مهربان و دلبسته به صاحب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Maltese'), N'ریزش موی بسیار کم', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Maltese'), N'حساس به تنهایی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Maltese'), N'نیاز به مراقبت روزانه از موی سفید', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'English Cocker Spaniel'), N'دوستانه و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Cocker Spaniel'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Cocker Spaniel'), N'مستعد عفونت گوش', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Cocker Spaniel'), N'نیاز به آرایش مرتب مو', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'American Cocker Spaniel'), N'شاد و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'American Cocker Spaniel'), N'وفادار به خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'American Cocker Spaniel'), N'مستعد عفونت گوش', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'American Cocker Spaniel'), N'نیاز به آرایش مرتب مو', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Cavalier King Charles Spaniel'), N'بسیار مهربان و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cavalier King Charles Spaniel'), N'خوب با کودکان و سایر حیوانات', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cavalier King Charles Spaniel'), N'مستعد بیماری قلبی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cavalier King Charles Spaniel'), N'حساس به تنهایی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Dachshund'), N'شجاع و کنجکاو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dachshund'), N'وفادار به صاحب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dachshund'), N'مستعد مشکلات دیسک کمر', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dachshund'), N'گاهی لجباز در آموزش', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Jack Russell Terrier'), N'پرانرژی و باهوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Jack Russell Terrier'), N'شجاع و بازیگوش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Jack Russell Terrier'), N'نیاز شدید به فعالیت روزانه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Jack Russell Terrier'), N'پارس زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Bull Terrier'), N'شوخ و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bull Terrier'), N'وفادار به خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bull Terrier'), N'لجباز و سرسخت در آموزش', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bull Terrier'), N'ممکن است با سگ‌های دیگر درگیر شود', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Chow Chow'), N'مستقل و باوقار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chow Chow'), N'محافظ خوب خانه', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chow Chow'), N'سرد نسبت به غریبه‌ها', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chow Chow'), N'نیاز به مراقبت زیاد از موی پرپشت', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Shar Pei'), N'وفادار و مستقل', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shar Pei'), N'آرام در خانه', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shar Pei'), N'مستعد مشکلات پوستی به‌خاطر چین‌وچروک', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Shar Pei'), N'ممکن است نسبت به غریبه‌ها بدبین باشد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Afghan Hound'), N'زیبا و باوقار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Afghan Hound'), N'مستقل و آرام', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Afghan Hound'), N'نیاز به مراقبت زیاد از موی بلند', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Afghan Hound'), N'سخت‌آموزش و مستقل', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Saluki'), N'برازنده و سریع', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Saluki'), N'وفادار و آرام', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Saluki'), N'تمایل زیاد به تعقیب و فرار', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Saluki'), N'حساس نسبت به غریبه‌ها', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Greyhound'), N'آرام و ملایم در خانه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Greyhound'), N'سریع‌ترین نژاد سگ', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Greyhound'), N'حساس به سرما به‌خاطر چربی بدن کم', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Greyhound'), N'غریزه شکار قوی نسبت به حیوانات کوچک', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Whippet'), N'آرام و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Whippet'), N'مناسب آپارتمان علی‌رغم سرعت بالا', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Whippet'), N'حساس به سرما', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Whippet'), N'غریزه شکار قوی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Newfoundland'), N'بسیار مهربان و صبور', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Newfoundland'), N'شناگر عالی و نجات‌غریق طبیعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Newfoundland'), N'آبریزش دهان زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Newfoundland'), N'نیاز به فضای زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Bernese Mountain Dog'), N'مهربان و صبور با خانواده', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bernese Mountain Dog'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bernese Mountain Dog'), N'عمر نسبتاً کوتاه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bernese Mountain Dog'), N'ریزش موی زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Miniature Schnauzer'), N'باهوش و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Miniature Schnauzer'), N'ریزش موی کم', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Miniature Schnauzer'), N'پارس زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Miniature Schnauzer'), N'نیاز به آرایش مرتب مو', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Standard Schnauzer'), N'باهوش و وفادار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Standard Schnauzer'), N'محافظ خوب خانه', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Standard Schnauzer'), N'نیاز به فعالیت زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Standard Schnauzer'), N'نیاز به آرایش مرتب مو', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Giant Schnauzer'), N'قدرتمند و باهوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Giant Schnauzer'), N'محافظ عالی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Giant Schnauzer'), N'نیاز به تربیت‌کننده باتجربه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Giant Schnauzer'), N'نیاز به فعالیت بدنی زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Bichon Frise'), N'شاد و دوست‌داشتنی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bichon Frise'), N'ریزش موی بسیار کم', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bichon Frise'), N'نیاز به آرایش مرتب مو', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bichon Frise'), N'حساس به تنهایی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Papillon'), N'باهوش و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Papillon'), N'اندازه کوچک مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Papillon'), N'شکننده در بازی با کودکان کوچک', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Papillon'), N'پارس زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Pekingese'), N'وفادار و مستقل', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pekingese'), N'مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pekingese'), N'مشکلات تنفسی پوزه‌کوتاه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pekingese'), N'حساس به گرما', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Pembroke Welsh Corgi'), N'باهوش و شاد', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pembroke Welsh Corgi'), N'وفادار به خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pembroke Welsh Corgi'), N'مستعد مشکلات کمر به‌خاطر بدن کشیده', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pembroke Welsh Corgi'), N'گرایش به گله‌کردن اعضای خانواده', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Cardigan Welsh Corgi'), N'باهوش و وفادار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cardigan Welsh Corgi'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cardigan Welsh Corgi'), N'مستعد مشکلات کمر', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cardigan Welsh Corgi'), N'پارس زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Irish Setter'), N'شاد و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Irish Setter'), N'مهربان با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Irish Setter'), N'انرژی بسیار بالا', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Irish Setter'), N'دیر بالغ می‌شود (رفتار توله‌سگی طولانی)', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'English Pointer'), N'باهوش و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Pointer'), N'وفادار و مهربان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Pointer'), N'نیاز شدید به فعالیت روزانه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Pointer'), N'برای آپارتمان کوچک مناسب نیست', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'English Springer Spaniel'), N'دوستانه و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Springer Spaniel'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Springer Spaniel'), N'مستعد عفونت گوش', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Springer Spaniel'), N'نیاز به فعالیت و آرایش مرتب', 0, 3, 0);
GO

INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Persian'), N'آرام و کم‌سروصدا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Persian'), N'زیبا و پرطرفدار', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Persian'), N'نیاز به شانه‌کشیدن روزانه موی بلند', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Persian'), N'مستعد مشکلات تنفسی به‌خاطر پوزه کوتاه', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'British Shorthair'), N'آرام و مستقل', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'British Shorthair'), N'مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'British Shorthair'), N'مستعد چاقی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'British Shorthair'), N'کم‌علاقه به بغل‌شدن زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'British Longhair'), N'آرام و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'British Longhair'), N'مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'British Longhair'), N'نیاز به شانه‌کشیدن مرتب مو', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'British Longhair'), N'مستعد چاقی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Scottish Fold'), N'مهربان و آرام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Scottish Fold'), N'ظاهر خاص و دوست‌داشتنی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Scottish Fold'), N'مستعد مشکلات مفصلی و غضروفی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Scottish Fold'), N'نیاز به معاینه دوره‌ای دامپزشکی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Scottish Straight'), N'مهربان و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Scottish Straight'), N'سازگار با خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Scottish Straight'), N'مستعد چاقی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Siamese'), N'بسیار اجتماعی و پرحرف', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siamese'), N'باهوش و کنجکاو', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siamese'), N'حساس به تنهایی طولانی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siamese'), N'صدای بلند و مداوم', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Maine Coon'), N'دوستانه و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Maine Coon'), N'خوب با کودکان و سایر حیوانات', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Maine Coon'), N'نیاز به شانه‌کشیدن مرتب موی بلند', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Maine Coon'), N'مستعد بیماری قلبی ژنتیکی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Ragdoll'), N'بسیار آرام و مطیع', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Ragdoll'), N'مهربان با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Ragdoll'), N'نیاز به مراقبت مرتب از مو', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Ragdoll'), N'مستعد بیماری قلبی ژنتیکی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Bengal'), N'پرانرژی و بازیگوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bengal'), N'ظاهر وحشی و زیبا', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bengal'), N'نیاز شدید به تحریک ذهنی', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bengal'), N'ممکن است خرابکار شود در صورت بی‌حوصلگی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Sphynx'), N'بسیار اجتماعی و پرمحبت', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Sphynx'), N'بدون ریزش مو', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Sphynx'), N'حساس به سرما و آفتاب', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Sphynx'), N'نیاز به حمام مرتب برای تمیزی پوست', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Russian Blue'), N'آرام و کمی خجالتی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Russian Blue'), N'وفادار به صاحب خود', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Russian Blue'), N'با غریبه‌ها کمی محتاط', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Turkish Angora'), N'باهوش و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Turkish Angora'), N'پرانرژی و بازیگوش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Turkish Angora'), N'برخی مبتلا به ناشنوایی ژنتیکی (رنگ سفید)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Turkish Van'), N'دوستدار آب و شنا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Turkish Van'), N'پرانرژی و بازیگوش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Turkish Van'), N'مستقل و گاهی سرسخت', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Abyssinian'), N'بسیار پرانرژی و کنجکاو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Abyssinian'), N'بازیگوش و اجتماعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Abyssinian'), N'نیاز به تحریک ذهنی زیاد', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'American Shorthair'), N'آرام و سازگار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'American Shorthair'), N'نگهداری آسان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'American Shorthair'), N'مستعد چاقی در صورت کم‌تحرکی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Exotic Shorthair'), N'آرام و کم‌سروصدا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Exotic Shorthair'), N'مراقبت موی راحت‌تر از پرشین', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Exotic Shorthair'), N'مشکلات تنفسی پوزه‌کوتاه', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Norwegian Forest Cat'), N'دوستانه و مستقل', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Norwegian Forest Cat'), N'مقاوم در برابر سرما', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Norwegian Forest Cat'), N'نیاز به شانه‌کشیدن مرتب مو', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Siberian'), N'مهربان و بازیگوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siberian'), N'آلرژی‌زایی کمتر نسبت به سایر نژادها', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siberian'), N'نیاز به مراقبت از موی بلند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Birman'), N'آرام و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Birman'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Birman'), N'نیاز به شانه‌کشیدن مرتب مو', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Burmese'), N'بسیار اجتماعی و دلبسته به صاحب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Burmese'), N'بازیگوش و پرانرژی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Burmese'), N'حساس به تنهایی طولانی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Bombay'), N'مهربان و آرام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bombay'), N'ظاهر پلنگی جذاب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bombay'), N'حساس به تنهایی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Oriental Shorthair'), N'بسیار اجتماعی و پرحرف', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Oriental Shorthair'), N'باهوش و کنجکاو', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Oriental Shorthair'), N'حساس به تنهایی طولانی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Devon Rex'), N'بازیگوش و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Devon Rex'), N'ریزش موی کم', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Devon Rex'), N'حساس به سرما', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Cornish Rex'), N'پرانرژی و بازیگوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cornish Rex'), N'موی نرم و مواج کم‌ریزش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cornish Rex'), N'حساس به سرما', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Selkirk Rex'), N'آرام و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Selkirk Rex'), N'موی فرفری خاص', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Selkirk Rex'), N'نیاز به مراقبت از موی فرفری', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Manx'), N'وفادار و بازیگوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Manx'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Manx'), N'مستعد مشکلات ستون فقرات (بی‌دمی)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan'), N'آرام و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan'), N'زیبا با چشمان آبی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan'), N'نیاز به شانه‌کشیدن روزانه', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan'), N'مشکلات تنفسی پوزه‌کوتاه', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Egyptian Mau'), N'پرانرژی و باهوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Egyptian Mau'), N'سریع‌ترین نژاد گربه خانگی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Egyptian Mau'), N'با غریبه‌ها خجالتی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Balinese'), N'اجتماعی و پرحرف', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Balinese'), N'باهوش و بازیگوش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Balinese'), N'حساس به تنهایی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Tonkinese'), N'اجتماعی و دلبسته به صاحب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tonkinese'), N'بازیگوش و باهوش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tonkinese'), N'حساس به تنهایی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Savannah'), N'پرانرژی و کنجکاو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Savannah'), N'ظاهر وحشی و چشمگیر', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Savannah'), N'نیاز به فضا و تحریک ذهنی زیاد', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Savannah'), N'نگهداری آن در برخی مناطق محدودیت قانونی دارد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Chartreux'), N'آرام و کم‌صدا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chartreux'), N'وفادار به خانواده', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chartreux'), N'با غریبه‌ها محتاط', 0, 2, 0);
GO

INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Holland Lop'), N'کوچک و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Holland Lop'), N'مناسب خانواده و کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Holland Lop'), N'گوش‌های آویزان مستعد عفونت', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Mini Lop'), N'بازیگوش و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Mini Lop'), N'اندازه کوچک مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Mini Lop'), N'نیاز به بررسی مرتب گوش', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Netherland Dwarf'), N'بسیار کوچک و بامزه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Netherland Dwarf'), N'کم‌جا مناسب فضای محدود', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Netherland Dwarf'), N'خجالتی و نیازمند رام‌سازی صبورانه', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Lionhead'), N'بازیگوش و کنجکاو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Lionhead'), N'ظاهر یال‌مانند خاص', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Lionhead'), N'نیاز به شانه‌کشیدن مرتب یال', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Flemish Giant'), N'آرام و صبور', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Flemish Giant'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Flemish Giant'), N'نیاز به فضا و غذای زیاد به‌خاطر جثه بزرگ', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Rex Rabbit'), N'موی مخملی نرم', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rex Rabbit'), N'آرام و اجتماعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rex Rabbit'), N'پوست حساس به‌خاطر موی کوتاه', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Mini Rex'), N'موی نرم و مخملی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Mini Rex'), N'اندازه کوچک و آرام', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Mini Rex'), N'پوست کف پا حساس', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'English Angora'), N'موی بسیار بلند و نرم', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Angora'), N'آرام و مهربان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Angora'), N'نیاز روزانه به شانه‌کشیدن موی بلند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'French Angora'), N'موی بلند و پرپشت', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'French Angora'), N'آرام و اجتماعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'French Angora'), N'نیاز به مراقبت مرتب از مو', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'English Lop'), N'آرام و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Lop'), N'گوش‌های بلند خاص و چشمگیر', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'English Lop'), N'گوش‌های بلند نیازمند مراقبت ویژه', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'French Lop'), N'آرام و بزرگ‌جثه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'French Lop'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'French Lop'), N'نیاز به فضای زیاد', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Dutch Rabbit'), N'دوستانه و باهوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dutch Rabbit'), N'مناسب مبتدیان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dutch Rabbit'), N'نیاز به فعالیت روزانه', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Californian Rabbit'), N'آرام و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Californian Rabbit'), N'خوب با کودکان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Californian Rabbit'), N'نیاز به فضای مناسب به‌خاطر جثه متوسط', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'New Zealand Rabbit'), N'آرام و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'New Zealand Rabbit'), N'مناسب مبتدیان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'New Zealand Rabbit'), N'نیاز به فضای مناسب', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan Rabbit'), N'آرام و کوچک', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan Rabbit'), N'مناسب مبتدیان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan Rabbit'), N'خجالتی در ابتدا', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Polish Rabbit'), N'کوچک و آرام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Polish Rabbit'), N'مناسب آپارتمان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Polish Rabbit'), N'خجالتی و نیازمند صبر', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Jersey Wooly'), N'کوچک و مهربان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Jersey Wooly'), N'موی پشمی نرم', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Jersey Wooly'), N'نیاز به مراقبت مرتب از مو', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Dwarf Hotot'), N'کوچک و بامزه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dwarf Hotot'), N'اجتماعی و کنجکاو', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dwarf Hotot'), N'خجالتی در ابتدا', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Harlequin Rabbit'), N'رنگ‌بندی خاص و چشمگیر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Harlequin Rabbit'), N'اجتماعی و بازیگوش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Harlequin Rabbit'), N'نیاز به فعالیت روزانه', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Chinchilla Rabbit'), N'موی نرم و متراکم', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chinchilla Rabbit'), N'آرام و اجتماعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chinchilla Rabbit'), N'نیاز به مراقبت مرتب از مو', 0, 2, 0);
GO

INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Budgerigar'), N'اجتماعی و آموزش‌پذیر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Budgerigar'), N'نگهداری آسان و ارزان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Budgerigar'), N'نیاز به تعامل روزانه برای جلوگیری از افسردگی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Cockatiel'), N'مهربان و آرام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cockatiel'), N'قابلیت سوت‌زدن و تقلید صدا', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cockatiel'), N'حساس به تنهایی طولانی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Lovebird'), N'بازیگوش و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Lovebird'), N'رنگ‌های زیبا', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Lovebird'), N'ممکن است نسبت به پرندگان دیگر پرخاشگر شود', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'African Grey Parrot'), N'بسیار باهوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'African Grey Parrot'), N'توانایی فوق‌العاده در تقلید کلام', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'African Grey Parrot'), N'نیاز شدید به تحریک ذهنی، وگرنه پرکنی می‌کند', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'African Grey Parrot'), N'عمر بسیار طولانی (تعهد چند دهه‌ای)', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Canary'), N'آواز زیبا و دلنشین', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Canary'), N'نگهداری آسان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Canary'), N'علاقه‌ای به دست‌گرفتن ندارد (فقط تماشایی)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Finch'), N'آرام و کم‌سروصدا نسبی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Finch'), N'مناسب نگهداری گروهی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Finch'), N'اهلی‌شدن و دست‌آموزی سخت', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Macaw'), N'باهوش و باوفا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Macaw'), N'زیبا و رنگارنگ', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Macaw'), N'صدای بسیار بلند', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Macaw'), N'نیاز به قفس بزرگ و فضای زیاد', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Cockatoo'), N'بسیار مهربان و دلبسته به صاحب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cockatoo'), N'باهوش و بازیگوش', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cockatoo'), N'صدای بسیار بلند و جیغ‌کشیدن', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cockatoo'), N'وابستگی شدید و حساس به تنهایی', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Amazon Parrot'), N'باهوش و توانا در تقلید کلام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Amazon Parrot'), N'شخصیت شاد و پرانرژی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Amazon Parrot'), N'ممکن است در فصل جفت‌گیری پرخاشگر شود', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Conure'), N'بازیگوش و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Conure'), N'رنگ‌های زیبا و شخصیت جذاب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Conure'), N'صدای نسبتاً بلند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Indian Ringneck Parakeet'), N'باهوش و توانا در تقلید کلام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Indian Ringneck Parakeet'), N'زیبا و باوقار', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Indian Ringneck Parakeet'), N'در دوره بلوغ ممکن است گازگیر شود', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Eclectus Parrot'), N'آرام نسبت به سایر طوطی‌های بزرگ', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Eclectus Parrot'), N'رنگ‌های بسیار چشمگیر', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Eclectus Parrot'), N'نیاز غذایی خاص (میوه و سبزی زیاد)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Caique'), N'بسیار بازیگوش و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Caique'), N'شخصیت بامزه و بانشاط', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Caique'), N'نیاز شدید به سرگرمی و بازی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Lorikeet'), N'بسیار پرانرژی و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Lorikeet'), N'رنگ‌های فوق‌العاده زیبا', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Lorikeet'), N'رژیم غذایی خاص (شهد و میوه)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Quaker Parrot'), N'باهوش و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Quaker Parrot'), N'توانا در یادگیری کلام', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Quaker Parrot'), N'در برخی کشورها نگهداری آن محدودیت دارد', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Senegal Parrot'), N'آرام‌تر از طوطی‌های بزرگ‌تر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Senegal Parrot'), N'وفادار به صاحب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Senegal Parrot'), N'ممکن است به یک نفر خاص وابسته شود', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Alexandrine Parakeet'), N'باهوش و توانا در صحبت', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Alexandrine Parakeet'), N'زیبا و باوقار', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Alexandrine Parakeet'), N'صدای بلند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Pigeon'), N'وفادار و آرام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pigeon'), N'توانایی شگفت‌انگیز مسیریابی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pigeon'), N'نیاز به فضای لانه مناسب', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Dove'), N'آرام و صلح‌جو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dove'), N'صدای ملایم و دلنشین', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dove'), N'خجالتی و کمتر دست‌آموز', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Java Sparrow'), N'آرام و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Java Sparrow'), N'ظاهر شیک و خاص', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Java Sparrow'), N'اهلی‌شدن نسبتاً سخت', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Zebra Finch'), N'پرانرژی و نگهداری آسان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Zebra Finch'), N'مناسب نگهداری گروهی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Zebra Finch'), N'علاقه‌ای به دست‌گرفتن ندارد', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Common Myna'), N'بسیار باهوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Common Myna'), N'توانایی عالی در تقلید صدا و کلام', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Common Myna'), N'صدای بلند و پرسروصدا', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Goldfinch'), N'آواز زیبا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Goldfinch'), N'ظاهر رنگارنگ جذاب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Goldfinch'), N'اهلی‌شدن سخت', 0, 2, 0);
GO

INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Goldfish'), N'مقاوم و مناسب مبتدیان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Goldfish'), N'عمر طولانی در نگهداری خوب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Goldfish'), N'آلاینده آب (نیاز به فیلتراسیون قوی)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Betta'), N'رنگ‌های بسیار زیبا و متنوع', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Betta'), N'نگهداری در تانک کوچک ممکن است', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Betta'), N'نرها نسبت به هم بسیار پرخاشگرند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Guppy'), N'مقاوم و آسان‌نگه‌دار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Guppy'), N'رنگ‌های متنوع و زیبا', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Guppy'), N'تکثیر بسیار سریع و جمعیت‌زیادشونده', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Molly'), N'مقاوم و سازگار با تانک اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Molly'), N'مناسب مبتدیان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Molly'), N'تکثیر سریع', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Platy'), N'آرام و سازگار با سایر ماهی‌ها', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Platy'), N'مقاوم و مناسب مبتدیان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Platy'), N'تکثیر سریع', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Swordtail'), N'پرانرژی و رنگارنگ', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Swordtail'), N'سازگار با تانک اجتماعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Swordtail'), N'نرها گاهی نسبت به هم پرخاشگر می‌شوند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Neon Tetra'), N'آرام و مناسب تانک اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Neon Tetra'), N'رنگ درخشان و زیبا', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Neon Tetra'), N'حساس به نوسان کیفیت آب', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Cardinal Tetra'), N'آرام و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cardinal Tetra'), N'رنگ درخشان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Cardinal Tetra'), N'حساس به کیفیت آب', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Angelfish'), N'زیبا و باوقار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Angelfish'), N'شخصیت جالب و قابل‌تشخیص', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Angelfish'), N'ممکن است ماهی‌های کوچک‌تر را شکار کند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Discus'), N'بسیار زیبا و رنگارنگ', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Discus'), N'آرام در تانک مناسب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Discus'), N'نیاز به کیفیت آب و دمای دقیق (نگهداری سخت)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Oscar'), N'باهوش و صاحب‌شناس', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Oscar'), N'شخصیت جالب و تعاملی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Oscar'), N'نیاز به تانک بسیار بزرگ', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Oscar'), N'پرخاشگر نسبت به ماهی‌های کوچک', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Zebra Danio'), N'بسیار مقاوم و آسان‌نگه‌دار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Zebra Danio'), N'پرانرژی و مناسب مبتدیان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Zebra Danio'), N'نیاز به نگهداری گروهی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Corydoras'), N'آرام و اجتماعی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Corydoras'), N'کمک به تمیزی کف تانک', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Corydoras'), N'نیاز به بستر نرم برای محافظت از سبیل', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Pleco'), N'کمک به کنترل جلبک تانک', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pleco'), N'آرام و بی‌آزار', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Pleco'), N'برخی گونه‌ها بسیار بزرگ می‌شوند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Koi'), N'زیبا و بادوام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Koi'), N'عمر بسیار طولانی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Koi'), N'نیاز به استخر بزرگ در فضای باز', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Clownfish'), N'زیبا و پرطرفدار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Clownfish'), N'نسبتاً مقاوم برای یک ماهی دریایی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Clownfish'), N'نیاز به تانک آب‌شور (هزینه و تجهیزات بیشتر)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Gourami'), N'آرام و زیبا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Gourami'), N'مناسب تانک اجتماعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Gourami'), N'نرها ممکن است نسبت به هم قلمرو‌طلب شوند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Rainbowfish'), N'رنگ‌های درخشان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rainbowfish'), N'آرام و اجتماعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rainbowfish'), N'نیاز به نگهداری گروهی', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Flowerhorn'), N'رنگ و شکل ظاهری خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Flowerhorn'), N'باهوش و صاحب‌شناس', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Flowerhorn'), N'بسیار پرخاشگر، باید تنها نگه‌داری شود', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Arowana'), N'زیبا و باوقار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Arowana'), N'نماد شانس در برخی فرهنگ‌ها', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Arowana'), N'نیاز به تانک بسیار بزرگ', 0, 2, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Arowana'), N'قابلیت پرش از تانک روباز', 0, 3, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Tiger Barb'), N'پرانرژی و رنگارنگ', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tiger Barb'), N'مقاوم و آسان‌نگه‌دار', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tiger Barb'), N'باله‌گیر (به ماهی‌های باله‌بلند حمله می‌کند)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Clown Loach'), N'شخصیت بازیگوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Clown Loach'), N'رنگ زیبا', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Clown Loach'), N'نیاز به نگهداری گروهی و تانک بزرگ', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Rainbow Shark'), N'ظاهر شارک‌مانند جذاب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rainbow Shark'), N'کمک به تمیزی کف تانک', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rainbow Shark'), N'قلمرو‌طلب نسبت به هم‌نوع', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'German Blue Ram'), N'رنگ‌های زیبا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'German Blue Ram'), N'آرام و مناسب تانک اجتماعی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'German Blue Ram'), N'حساس به کیفیت و دمای آب', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'African Cichlid'), N'رنگ‌های بسیار متنوع', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'African Cichlid'), N'شخصیت پرانرژی و جالب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'African Cichlid'), N'قلمرو‌طلب و گاهی پرخاشگر', 0, 2, 0);
GO

INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Syrian Hamster'), N'مناسب مبتدیان و نگهداری تکی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Syrian Hamster'), N'بزرگ‌تر و راحت‌تر برای دست‌گرفتن', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Syrian Hamster'), N'باید تنها نگه‌داری شود (با هم‌نوع درگیر می‌شود)', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Roborovski Hamster'), N'بسیار فعال و پرانرژی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Roborovski Hamster'), N'کوچک و بامزه', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Roborovski Hamster'), N'سریع و دشوار برای دست‌گرفتن', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Campbell Dwarf Hamster'), N'قابل نگهداری گروهی در برخی موارد', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Campbell Dwarf Hamster'), N'کوچک و فعال', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Campbell Dwarf Hamster'), N'ممکن است گاز بگیرد اگر ناگهانی دست بخورد', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Winter White Hamster'), N'آرام‌تر از روبوروفسکی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Winter White Hamster'), N'ظاهر بامزه با تغییر رنگ فصلی', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Winter White Hamster'), N'شب‌فعال و ممکن است هنگام روز خواب باشد', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Chinese Hamster'), N'آرام‌تر و کمی اجتماعی‌تر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chinese Hamster'), N'بدن باریک و متفاوت', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chinese Hamster'), N'پیداکردنش در بازار سخت‌تر است', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Teddy Bear Hamster'), N'موی بلند و نرم', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Teddy Bear Hamster'), N'آرام و مناسب مبتدیان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Teddy Bear Hamster'), N'نیاز به تمیزکردن مرتب موی بلند', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Black Bear Hamster'), N'رنگ مشکی خاص و چشمگیر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Black Bear Hamster'), N'آرام مانند سایر سوری‌ها', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Black Bear Hamster'), N'باید تنها نگه‌داری شود', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Panda Bear Hamster'), N'رنگ سیاه‌وسفید بامزه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Panda Bear Hamster'), N'آرام و مناسب مبتدیان', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Panda Bear Hamster'), N'باید تنها نگه‌داری شود', 0, 2, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'European Hamster'), N'اندازه بزرگ‌تر از سایر همسترها', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'European Hamster'), N'ظاهر متفاوت و جالب', 1, 1, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'European Hamster'), N'در برخی کشورها گونه حفاظت‌شده و کمیاب است', 0, 2, 0);
GO

-- خوکچه هندی
INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'American Guinea Pig'), N'آرام و مناسب مبتدیان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'American Guinea Pig'), N'موی کوتاه و نگهداری آسان', 1, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Abyssinian Guinea Pig'), N'شخصیت پرانرژی و کنجکاو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Abyssinian Guinea Pig'), N'موی مارپیچی نیازمند مراقبت بیشتر', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Peruvian Guinea Pig'), N'موی بسیار بلند و زیبا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Peruvian Guinea Pig'), N'نیاز روزانه به شانه‌کشیدن مو', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Silkie Guinea Pig'), N'موی صاف و ابریشمی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Silkie Guinea Pig'), N'نیاز به مراقبت مرتب از مو', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Texel Guinea Pig'), N'موی فرفری و خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Texel Guinea Pig'), N'نیاز زیاد به مراقبت از مو', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Coronet Guinea Pig'), N'ظاهر خاص با تاج مویی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Coronet Guinea Pig'), N'نیاز به مراقبت مرتب از مو', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Skinny Pig'), N'بدون مو، مناسب افراد حساس به مو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Skinny Pig'), N'حساس به سرما و آفتاب', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'White Crested Guinea Pig'), N'ظاهر ساده و نگهداری آسان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'White Crested Guinea Pig'), N'کمیاب‌تر نسبت به سایر نژادها', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Rex Guinea Pig'), N'موی ضخیم و مواج', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rex Guinea Pig'), N'نیاز به مراقبت متوسط از مو', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Teddy Guinea Pig'), N'موی کوتاه و ضخیم بامزه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Teddy Guinea Pig'), N'نگهداری نسبتاً آسان', 1, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan Guinea Pig'), N'رنگ‌بندی خاص شبیه گربه هیمالین', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan Guinea Pig'), N'کمیاب و کمتر در بازار موجود', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Merino Guinea Pig'), N'موی فرفری بلند خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Merino Guinea Pig'), N'نیاز زیاد به مراقبت از مو', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Lunkarya Guinea Pig'), N'موی فرفری بسیار خاص و کمیاب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Lunkarya Guinea Pig'), N'نیاز زیاد به مراقبت از مو و پیداکردن سخت', 0, 1, 0);
GO

-- لاک‌پشت / تستودو
INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Red-Eared Slider'), N'مقاوم و مناسب مبتدیان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Red-Eared Slider'), N'نیاز به تانک آب بزرگ و نور UVB', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Yellow-Bellied Slider'), N'فعال و کنجکاو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Yellow-Bellied Slider'), N'نیاز به فضای شنای بزرگ', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Painted Turtle'), N'رنگ‌های زیبا روی لاک', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Painted Turtle'), N'نیاز به نور UVB و دمای دقیق', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Map Turtle'), N'طرح لاک شبیه نقشه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Map Turtle'), N'خجالتی و نیازمند صبر برای اهلی‌شدن', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Musk Turtle'), N'اندازه کوچک مناسب تانک متوسط', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Musk Turtle'), N'ترشح بوی مشک هنگام استرس', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Mud Turtle'), N'اندازه کوچک و نگهداری نسبتاً آسان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Mud Turtle'), N'خجالتی و کمتر فعال', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Russian Tortoise'), N'کوچک و مناسب نگهداری خانگی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Russian Tortoise'), N'نیاز به دمای گرم و نور UVB', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Greek Tortoise'), N'کوچک و آرام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Greek Tortoise'), N'عمر بسیار طولانی (تعهد چند دهه‌ای)', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Sulcata Tortoise'), N'قوی و بادوام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Sulcata Tortoise'), N'در بزرگسالی بسیار غول‌پیکر و نیازمند فضای زیاد می‌شود', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Hermann s Tortoise'), N'آرام و کوچک', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Hermann s Tortoise'), N'نیاز به محیط باغچه‌ای یا فضای باز', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Box Turtle'), N'شخصیت جالب و کنجکاو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Box Turtle'), N'رژیم غذایی متنوع و نیازمند دقت', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Snapping Turtle'), N'بسیار مقاوم و بادوام', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Snapping Turtle'), N'گاز بسیار قوی و خطرناک، مناسب مبتدیان نیست', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Softshell Turtle'), N'شناگر بسیار سریع', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Softshell Turtle'), N'پوست حساس و عصبی‌مزاج', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Reeve s Turtle'), N'اندازه کوچک مناسب آپارتمان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Reeve s Turtle'), N'نیاز به نگهداری دقیق کیفیت آب', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Indian Star Tortoise'), N'طرح لاک ستاره‌ای بسیار زیبا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Indian Star Tortoise'), N'نگهداری آن در بسیاری کشورها محدودیت قانونی دارد', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Leopard Tortoise'), N'طرح لاک پلنگی زیبا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Leopard Tortoise'), N'در بزرگسالی جثه بزرگی پیدا می‌کند', 0, 1, 0);
GO

-- خزنده
INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Bearded Dragon'), N'آرام و مناسب مبتدیان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Bearded Dragon'), N'نیاز به نور UVB و دمای دقیق', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Leopard Gecko'), N'آرام و نگهداری آسان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Leopard Gecko'), N'شب‌فعال', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Crested Gecko'), N'نگهداری آسان و بدون نیاز به UVB قوی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Crested Gecko'), N'دُم قابل قطع‌شدن در استرس (دیگر رشد نمی‌کند)', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Blue-Tongue Skink'), N'آرام و باهوش', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Blue-Tongue Skink'), N'نیاز به تراریوم بزرگ', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Ball Python'), N'آرام و مناسب مبتدیان مارنگهدار', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Ball Python'), N'گاهی غذا نخوردن طولانی', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Corn Snake'), N'آرام و نگهداری بسیار آسان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Corn Snake'), N'توانایی فرار از قفس‌های نامناسب', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Green Iguana'), N'زیبا و چشمگیر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Green Iguana'), N'در بزرگسالی بزرگ و نیازمند فضای زیاد می‌شود', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Veiled Chameleon'), N'ظاهر بسیار خاص و رنگ‌تغییردهنده', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Veiled Chameleon'), N'نیازمند شرایط نگهداری دقیق و حساس به استرس', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Tegu'), N'باهوش و قابل رام‌شدن', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tegu'), N'در بزرگسالی جثه بزرگ پیدا می‌کند', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Uromastyx'), N'آرام و روزفعال', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Uromastyx'), N'نیاز به دمای بسیار بالا در تراریوم', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'King Snake'), N'آرام و نگهداری آسان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'King Snake'), N'نباید با مار دیگر هم‌قفس شود (ممکن است شکار کند)', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Milk Snake'), N'رنگ‌های زیبا و چشمگیر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Milk Snake'), N'نیازمند قفس کاملاً امن به‌خاطر توانایی فرار', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Boa Constrictor'), N'آرام نسبت به جثه بزرگ', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Boa Constrictor'), N'نیاز به تراریوم بزرگ و مناسب مبتدی نیست', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Chinese Water Dragon'), N'زیبا و فعال', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Chinese Water Dragon'), N'نیازمند تراریوم بزرگ با بخش آبی', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Green Anole'), N'کوچک و نگهداری آسان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Green Anole'), N'حساس و کمتر مناسب دست‌گرفتن مکرر', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'House Gecko'), N'بسیار کوچک و کم‌نیاز', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'House Gecko'), N'سریع و دشوار برای دست‌گرفتن', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Tokay Gecko'), N'رنگ‌های زیبا و چشمگیر', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tokay Gecko'), N'پرخاشگر و گازگیر، مناسب مبتدی نیست', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Savannah Monitor'), N'باهوش و قابل رام‌شدن', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Savannah Monitor'), N'در بزرگسالی جثه بزرگ و نیازمند تراریوم وسیع', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Garter Snake'), N'کوچک و مناسب مبتدیان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Garter Snake'), N'ممکن است بوی دفاعی از خود ساطع کند', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Ackie Monitor'), N'نسبتاً کوچک برای یک مانیتور', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Ackie Monitor'), N'نیاز به تراریوم بزرگ با بستر حفاری', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Frilled Dragon'), N'ظاهر خاص و چشمگیر با یقه', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Frilled Dragon'), N'ترسو و مستعد استرس', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Gargoyle Gecko'), N'نگهداری آسان مشابه کرستد گکو', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Gargoyle Gecko'), N'شب‌فعال', 0, 1, 0);
GO

-- جوجه‌تیغی
INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'African Pygmy Hedgehog'), N'کوچک و مناسب نگهداری خانگی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'African Pygmy Hedgehog'), N'شب‌فعال و خارپشتی هنگام ترس', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Algerian Hedgehog'), N'اندازه کمی بزرگ‌تر از پیگمی آفریقایی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Algerian Hedgehog'), N'کمتر در بازار پت موجود', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Long-Eared Hedgehog'), N'گوش‌های بزرگ خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Long-Eared Hedgehog'), N'نگهداری آن نادر و اطلاعات کمتر در دسترس است', 0, 1, 0);
GO

-- شینچیلا (رنگ)
INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Standard Grey Chinchilla'), N'رایج‌ترین و در دسترس‌ترین رنگ', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Standard Grey Chinchilla'), N'نیاز به حمام شن مرتب برای سلامت پوست', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'White Chinchilla'), N'رنگ سفید نایاب و زیبا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'White Chinchilla'), N'قیمت بالاتر نسبت به رنگ استاندارد', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Beige Chinchilla'), N'رنگ گرم و متفاوت', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Beige Chinchilla'), N'کمیاب‌تر از رنگ استاندارد', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Black Velvet Chinchilla'), N'رنگ تیره و بسیار زیبا', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Black Velvet Chinchilla'), N'قیمت بالاتر', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Violet Chinchilla'), N'رنگ بنفش‌مایل کمیاب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Violet Chinchilla'), N'قیمت بالا و کمیاب', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Sapphire Chinchilla'), N'رنگ آبی‌خاکستری خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Sapphire Chinchilla'), N'کمیاب و گران‌تر', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Ebony Chinchilla'), N'رنگ مشکی یکدست', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Ebony Chinchilla'), N'کمیاب‌تر از رنگ استاندارد', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Tan Chinchilla'), N'رنگ روشن و متفاوت', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tan Chinchilla'), N'نسبتاً کمیاب', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Wilson White Chinchilla'), N'رنگ سفید خالص خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Wilson White Chinchilla'), N'بسیار کمیاب و گران', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Homo Beige Chinchilla'), N'رنگ بژ روشن یکدست', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Homo Beige Chinchilla'), N'کمیاب‌تر از بژ معمولی', 0, 1, 0);
GO

-- موش فانتزی
INSERT INTO [dbo].[PetBreedCharacteristics] (PetBreedId, Name, IsPositive, Priority, Deleted) VALUES
((SELECT Id FROM PetBreeds WHERE Label=N'Standard Fancy Mouse'), N'نگهداری بسیار آسان و ارزان', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Standard Fancy Mouse'), N'عمر نسبتاً کوتاه', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Satin Mouse'), N'موی براق و نرم', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Satin Mouse'), N'عمر نسبتاً کوتاه', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Long-Haired Mouse'), N'موی بلند و خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Long-Haired Mouse'), N'نیاز به مراقبت بیشتر از مو', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Rex Mouse'), N'موی مواج و متفاوت', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Rex Mouse'), N'کمیاب‌تر از نوع استاندارد', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan Mouse'), N'رنگ‌بندی خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Himalayan Mouse'), N'کمیاب‌تر در بازار', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Siamese Mouse'), N'رنگ‌بندی جذاب مشابه گربه سیامی', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Siamese Mouse'), N'کمیاب‌تر در بازار', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Dutch Mouse'), N'طرح رنگی خاص و متقارن', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Dutch Mouse'), N'یافتن نمونه با طرح دقیق سخت است', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Fuzzy Mouse'), N'موی بسیار کم و ظاهر خاص', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Fuzzy Mouse'), N'حساس به سرما به‌خاطر موی کم', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Tan Mouse'), N'رنگ‌بندی دورنگ جذاب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Tan Mouse'), N'عمر نسبتاً کوتاه', 0, 1, 0),

((SELECT Id FROM PetBreeds WHERE Label=N'Merle Mouse'), N'طرح رنگی ابری خاص و کمیاب', 1, 0, 0),
((SELECT Id FROM PetBreeds WHERE Label=N'Merle Mouse'), N'کمیاب و گران‌تر', 0, 1, 0);
GO
