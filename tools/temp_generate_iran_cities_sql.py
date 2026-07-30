#!/usr/bin/env python3
from __future__ import annotations
import json, math, re, unicodedata, urllib.request, zipfile
from pathlib import Path
from typing import Any
SOURCE_URL='https://raw.githubusercontent.com/hemedani/yademan/b05b9ada40f94b72ee5104453392c31bdca0a13d/back/assets/geojson/irn_admin2_simplified_converted.json'
OUTPUT_SQL=Path('artifacts/Insert_Iran_Cities_Offline.sql')
OUTPUT_ZIP=Path('artifacts/Insert_Iran_Cities_Offline.zip')
EXPECTED_FEATURES=432
STATE_IDS={'آذربایجان شرقی':1,'آذربایجان غربی':2,'اردبیل':3,'اصفهان':4,'ایلام':5,'بوشهر':6,'تهران':7,'چهارمحال و بختیاری':8,'البرز':9,'خوزستان':10,'زنجان':11,'سمنان':12,'سیستان و بلوچستان':13,'فارس':14,'کرمان':15,'کردستان':16,'کرمانشاه':17,'کهگیلویه و بویراحمد':18,'گیلان':19,'لرستان':20,'مازندران':21,'مرکزی':22,'هرمزگان':23,'همدان':24,'یزد':25,'قم':26,'گلستان':27,'قزوین':28,'خراسان جنوبی':29,'خراسان رضوی':30,'خراسان شمالی':31}
def nf(v:Any)->str:
 t=unicodedata.normalize('NFKC',str(v or '')); t=t.replace('ي','ی').replace('ى','ی').replace('ك','ک').replace('ۀ','ه').replace('ة','ه').replace('\u200c',' '); return re.sub(r'\s+',' ',t).strip()
def np(v:Any)->str:
 n=nf(v); n=n[6:].strip() if n.startswith('استان ') else n; return {'چهار محال و بختیاری':'چهارمحال و بختیاری','کهگیلویه وبویراحمد':'کهگیلویه و بویراحمد','کهگیلویه و بویر احمد':'کهگیلویه و بویراحمد'}.get(n,n)
def nc(v:Any)->str:
 n=nf(v); return n[len('شهرستان '):].strip() if n.startswith('شهرستان ') else n
def esc(s:str)->str:return s.replace("'","''")
def num(v:Any)->str:
 x=float(v)
 if not math.isfinite(x): raise ValueError(v)
 x=round(x,7)
 if x==0:return '0'
 return f'{x:.7f}'.rstrip('0').rstrip('.')
def close(r):
 q=[list(p[:2]) for p in r]
 if q[0]!=q[-1]:q.append(q[0])
 return q
def area(r):return sum(float(a[0])*float(b[1])-float(b[0])*float(a[1]) for a,b in zip(r,r[1:]))/2
def ring(r,ccw):
 q=close(r)
 if (area(q)>0)!=ccw:q=list(reversed(q))
 return '('+','.join(f'{num(p[0])} {num(p[1])}' for p in q)+')'
def poly(c):return '('+','.join([ring(c[0],True)]+[ring(x,False) for x in c[1:]])+')'
def wkt(g):
 t=g['type']; c=g['coordinates']
 return ('POLYGON'+poly(c)) if t=='Polygon' else ('MULTIPOLYGON('+','.join(poly(p) for p in c)+')')
def assign(s,chunk=3500):
 cs=[esc(s[i:i+chunk]) for i in range(0,len(s),chunk)]
 return "        SET @Wkt = CONVERT(nvarchar(max), N'"+cs[0]+"');\n"+'\n'.join("        SET @Wkt += N'"+x+"';" for x in cs[1:])
def main():
 req=urllib.request.Request(SOURCE_URL,headers={'User-Agent':'Pastil-SQL-Builder/1.0'})
 with urllib.request.urlopen(req,timeout=120) as r:data=json.loads(r.read().decode('utf-8-sig'))
 fs=data['features']
 if len(fs)!=EXPECTED_FEATURES:raise ValueError(f'Expected 432, got {len(fs)}')
 rows=[]; sm={np(k):v for k,v in STATE_IDS.items()}
 for f in fs:
  p=f['properties']; prov=np(p.get('adm1_name1') or p.get('adm1_name')); city=nc(p.get('adm2_name1') or p.get('adm2_name')); rows.append((sm[prov],city,nf(p.get('adm2_pcode')),wkt(f['geometry'])))
 rows.sort(key=lambda x:(x[0],x[1]))
 blocks=[]
 for sid,city,code,shape in rows:
  blocks.append(f"        -- {esc(code)} | {esc(city)}\n{assign(shape)}\n        SET @Boundary = geography::STGeomFromText(@Wkt, 4326).MakeValid();\n        IF @Boundary.EnvelopeAngle() > 90 SET @Boundary = @Boundary.ReorientObject();\n        INSERT INTO [dbo].[Cities] ([StateId],[Boundary],[Name]) VALUES ({sid},@Boundary,N'{esc(city)}');\n")
 body='\n'.join(blocks)
 sql=f'''/* Pastil Iran counties as cities\nEmbedded records: {len(rows)}\nOffline SQL, SRID 4326\nWARNING: deletes all rows from dbo.Cities first. */\nUSE [pastil_db];\nGO\nSET NOCOUNT ON; SET XACT_ABORT ON;\nGO\nBEGIN TRY\n BEGIN TRANSACTION;\n IF (SELECT COUNT(*) FROM dbo.States)<>31 THROW 50001,N'تعداد استان‌ها برابر ۳۱ نیست.',1;\n DELETE FROM dbo.Cities;\n DBCC CHECKIDENT (N'dbo.Cities', RESEED, 0) WITH NO_INFOMSGS;\n DECLARE @Wkt nvarchar(max), @Boundary geography;\n{body}\n IF (SELECT COUNT(*) FROM dbo.Cities)<>{len(rows)} THROW 50002,N'تعداد شهرهای درج‌شده نادرست است.',1;\n IF EXISTS(SELECT 1 FROM dbo.Cities WHERE Boundary IS NULL OR Boundary.STSrid<>4326 OR Boundary.STIsValid()<>1) THROW 50003,N'اعتبارسنجی محدوده‌ها ناموفق بود.',1;\n COMMIT;\nEND TRY\nBEGIN CATCH\n IF @@TRANCOUNT>0 ROLLBACK;\n THROW;\nEND CATCH;\nGO\nSELECT s.Id StateId,s.Name StateName,COUNT(c.Id) CityCount,SUM(CASE WHEN c.Boundary IS NOT NULL THEN 1 ELSE 0 END) CityBoundaryCount FROM dbo.States s LEFT JOIN dbo.Cities c ON c.StateId=s.Id GROUP BY s.Id,s.Name ORDER BY s.Id;\nGO\n'''
 OUTPUT_SQL.parent.mkdir(exist_ok=True); OUTPUT_SQL.write_text(sql,encoding='utf-8-sig')
 with zipfile.ZipFile(OUTPUT_ZIP,'w',zipfile.ZIP_DEFLATED) as z:z.write(OUTPUT_SQL,OUTPUT_SQL.name)
 print(len(rows),OUTPUT_SQL.stat().st_size)
if __name__=='__main__':main()
