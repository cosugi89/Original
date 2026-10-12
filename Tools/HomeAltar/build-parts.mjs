// 元祭壇の見える画素を物体輪郭ごとに一度だけ所有させる。矩形は保存時の余白削除にだけ使う。
// sharp は標準の依存解決、または CODEX_NODE_MODULES で指定したライブラリを使う。
import fs from 'node:fs';
import path from 'node:path';
import {createRequire} from 'node:module';
const require=createRequire(process.env.CODEX_NODE_MODULES ? path.join(process.env.CODEX_NODE_MODULES,'_entry.cjs') : import.meta.url);
const sharp=require('sharp');
const project=path.resolve(path.dirname(new URL(import.meta.url).pathname.replace(/^\/(?:([A-Za-z]:))/, '$1')), '../..');
const sourcePath=path.join(project,'Assets/Resources/Sprite/ChatGPT 画像 2026年10月4日 12_40_51.png');
const output=path.join(project,'Assets/Resources/Sprite/Home/AltarBuild/Individual');
const qa=path.join(project,'Temp/HomeEntrance');
fs.mkdirSync(output,{recursive:true});fs.mkdirSync(qa,{recursive:true});
const {data:source,info}=await sharp(sourcePath).ensureAlpha().raw().toBuffer({resolveWithObject:true});
const w=info.width,h=info.height,pixelCount=w*h;
const stones=JSON.parse(fs.readFileSync(path.join(project,'Tools/HomeAltar/stones.json')));
const clusters=JSON.parse(fs.readFileSync(path.join(project,'Tools/HomeAltar/leaves.json')));
const leaves=clusters.flatMap(c=>c.tips.map((tip,i)=>({name:`Leaf_${c.name}_${String(i+1).padStart(2,'0')}`,root:c.root,tip,cluster:c.name})));
const masks=await Promise.all(['Left','Right'].map(async side=>(await sharp(path.join(project,`Assets/Resources/Sprite/Home/AltarBuild/Altar_Foliage_${side}.png`)).ensureAlpha().raw().toBuffer())));
const restored=await sharp(path.join(project,'Tools/HomeAltar/restored-underlay.png')).ensureAlpha().raw().toBuffer();
const closedAtlas=await sharp(path.join(project,'Tools/HomeAltar/closed-stones-atlas.png')).ensureAlpha().raw().toBuffer({resolveWithObject:true});
// 旧マスクは緑の塗りだけを持つため、葉の太い黒輪郭も近傍から回収する。
const leafMask=new Uint8Array(pixelCount);
for(let p=0;p<pixelCount;p++)if(masks.some(m=>m[p*4+3]>0))leafMask[p]=1;
const seedMask=leafMask.slice();
for(let p=0;p<pixelCount;p++){
  if(leafMask[p]||!source[p*4+3]||Math.max(source[p*4],source[p*4+1],source[p*4+2])>85)continue;
  const x=p%w,y=Math.floor(p/w);let found=false;
  for(let dy=-7;dy<=7&&!found;dy++)for(let dx=-7;dx<=7;dx++)if(dx*dx+dy*dy<=49&&x+dx>=0&&x+dx<w&&y+dy>=0&&y+dy<h&&seedMask[(y+dy)*w+x+dx]){found=true;break;}
  if(found)leafMask[p]=1;
}
// 旧色選択が葉として拾った、右柱足元の丸い小石を除外する。
for(let y=636;y<=660;y++)for(let x=926;x<=950;x++)if((x-938)**2+(y-648)**2<=144)leafMask[y*w+x]=0;
const owners=new Int16Array(pixelCount).fill(-1), polygons=stones.map(s=>s.polygon);
const leafStarts=clusters.map(c=>leaves.findIndex(l=>l.cluster===c.name));
function contains(poly,x,y){let inside=false;for(let i=0,j=poly.length-1;i<poly.length;j=i++){const [ax,ay]=poly[i],[bx,by]=poly[j];if((ay>y)!==(by>y)&&x<(bx-ax)*(y-ay)/(by-ay)+ax)inside=!inside;}return inside;}
function distance(poly,x,y){let min=Infinity;for(let i=0,j=poly.length-1;i<poly.length;j=i++){const [ax,ay]=poly[i],[bx,by]=poly[j],vx=bx-ax,vy=by-ay;const t=Math.max(0,Math.min(1,((x-ax)*vx+(y-ay)*vy)/(vx*vx+vy*vy)));min=Math.min(min,(x-ax-t*vx)**2+(y-ay-t*vy)**2);}return min;}
let uncontained=0;
for(let p=0;p<pixelCount;p++){
  if(!source[p*4+3])continue;
  const x=p%w,y=Math.floor(p/w);
  if(leafMask[p]){
    let ci=0,nearest=Infinity;
    clusters.forEach((c,i)=>{const d=(x-c.root[0])**2+(y-c.root[1])**2;if(d<nearest){nearest=d;ci=i;}});
    const c=clusters[ci],radius=Math.max(...c.tips.map(t=>Math.hypot(t[0]-c.root[0],t[1]-c.root[1])))+15;
    if(nearest<=radius*radius){
      let leafIndex=0,delta=Infinity;
      leaves.forEach((l,i)=>{const [ax,ay]=l.root,[bx,by]=l.tip,vx=bx-ax,vy=by-ay;const t=Math.max(0,Math.min(1,((x-ax)*vx+(y-ay)*vy)/(vx*vx+vy*vy)));const d=(x-ax-t*vx)**2+(y-ay-t*vy)**2;if(d<delta){delta=d;leafIndex=i;}});
      owners[p]=stones.length+leafIndex;continue;
    }
  }
  let owner=-1;
  for(let i=0;i<stones.length;i++)if(contains(polygons[i],x+.5,y+.5)&&(owner<0||stones[i].order>stones[owner].order))owner=i;
  if(owner<0){uncontained++;let min=Infinity;polygons.forEach((poly,i)=>{const d=distance(poly,x+.5,y+.5);if(d<min){min=d;owner=i;}});}
  owners[p]=owner;
}
// 石の縁など離れた黒い断片を葉へ巻き込まない。葉本体の連結成分だけを保つ。
for(let li=0;li<leaves.length;li++){
  const owner=stones.length+li,seen=new Uint8Array(pixelCount),components=[];
  for(let p=0;p<pixelCount;p++)if(owners[p]===owner&&!seen[p]&&source[p*4+3]>=8){
    const q=[p];seen[p]=1;
    for(let n=0;n<q.length;n++){const v=q[n],x=v%w,y=Math.floor(v/w);for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){const k=(y+dy)*w+x+dx;if(x+dx>=0&&x+dx<w&&y+dy>=0&&y+dy<h&&!seen[k]&&owners[k]===owner&&source[k*4+3]>=8){seen[k]=1;q.push(k);}}}
    components.push(q);
  }
  components.sort((a,b)=>b.length-a.length);
  for(const component of components.slice(1))for(const p of component){const x=p%w,y=Math.floor(p/w);let min=Infinity,nearest=0;polygons.forEach((poly,i)=>{const d=distance(poly,x+.5,y+.5);if(d<min){min=d;nearest=i;}});owners[p]=nearest;}
}
const entries=[],buffers=[],assembledComposite=Buffer.alloc(source.length),fullComposite=Buffer.alloc(source.length);
for(let i=0;i<stones.length+leaves.length;i++){
  const isLeaf=i>=stones.length,definition=isLeaf?leaves[i-stones.length]:stones[i];
  const fitted=Buffer.alloc(source.length),full=Buffer.alloc(source.length);
  let xmin=w,ymin=h,xmax=-1,ymax=-1,visiblePixels=0,restoredPixels=0;
  for(let p=0;p<pixelCount;p++){
    const x=p%w,y=Math.floor(p/w),b=p*4;
    if(owners[p]===i){source.copy(fitted,b,b,b+4);source.copy(full,b,b,b+4);source.copy(assembledComposite,b,b,b+4);visiblePixels++;}
    // 補完は当該石の輪郭内で、後に別物体・葉で覆われる画素に限定する。
    else if(!isLeaf&&owners[p]>=0&&contains(definition.polygon,x+.5,y+.5)&&restored[b+3]>128&&
      (owners[p]>=stones.length||stones[owners[p]].order>definition.order)){
      restored.copy(full,b,b,b+4);restoredPixels++;
    }
    if(full[b+3]){xmin=Math.min(xmin,x);xmax=Math.max(xmax,x);ymin=Math.min(ymin,y);ymax=Math.max(ymax,y);}
  }
  if(xmax<0)throw new Error(`空のSprite: ${definition.name}`);
  let mx=w,my=h,mxx=0,myy=0;
  for(let p=0;p<pixelCount;p++)if(full[p*4+3]>=16){const x=p%w,y=Math.floor(p/w);mx=Math.min(mx,x);my=Math.min(my,y);mxx=Math.max(mxx,x);myy=Math.max(myy,y);}
  if(!isLeaf&&i<36){
    // 完全輪郭の補描atlasを元の物体の実寸へ登録し、既存の見える面をその上に保存する。
    const cw=closedAtlas.info.width/6,ch=closedAtlas.info.height/6;
    const tile=await sharp(closedAtlas.data,{raw:closedAtlas.info}).extract({left:Math.round((i%6)*cw),top:Math.round(Math.floor(i/6)*ch),width:Math.floor(cw),height:Math.floor(ch)}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
    let tx=tile.info.width,ty=tile.info.height,txx=0,tyy=0;
    for(let p=0;p<tile.info.width*tile.info.height;p++)if(tile.data[p*4+3]>32){const x=p%tile.info.width,y=Math.floor(p/tile.info.width);tx=Math.min(tx,x);ty=Math.min(ty,y);txx=Math.max(txx,x);tyy=Math.max(tyy,y);}
    const body=await sharp(tile.data,{raw:tile.info}).extract({left:tx,top:ty,width:txx-tx+1,height:tyy-ty+1}).resize({width:mxx-mx+1,height:myy-my+1,fit:'fill'}).raw().toBuffer();
    full.fill(0);
    for(let y=my;y<=myy;y++)for(let x=mx;x<=mxx;x++){const b=(y*w+x)*4,from=((y-my)*(mxx-mx+1)+x-mx)*4;body.copy(full,b,from,from+4);}
    // 見える面は輪郭を含め元画素のまま保ち、補完面は後に置く不透明物体の下へだけ描く。
    // 同じSpriteを保持したまま、後続の石・葉の重なりで完成画像へ一致する。
    for(let p=0;p<pixelCount;p++){
      const b=p*4;
      if(owners[p]===i)source.copy(full,b,b,b+4);
      else if(owners[p]<=i||source[b+3]!==255)full.fill(0,b,b+4);
    }
  }
  if(isLeaf){xmin=Math.min(xmin,definition.root[0]);xmax=Math.max(xmax,definition.root[0]);ymin=Math.min(ymin,definition.root[1]);ymax=Math.max(ymax,definition.root[1]);}
  const bbox={left:Math.max(0,xmin-2),top:Math.max(0,ymin-2),width:Math.min(w-1,xmax+2)-Math.max(0,xmin-2)+1,height:Math.min(h-1,ymax+2)-Math.max(0,ymin-2)+1};
  const sprite=`${definition.name}.png`,assembled=isLeaf?sprite:`${definition.name}_Assembled.png`;
  await sharp(full,{raw:{width:w,height:h,channels:4}}).extract(bbox).png().toFile(path.join(output,sprite));
  if(!isLeaf)await sharp(fitted,{raw:{width:w,height:h,channels:4}}).extract(bbox).png().toFile(path.join(output,assembled));
  const pivotPx=isLeaf?definition.root[0]:bbox.left+bbox.width/2,pivotPy=isLeaf?definition.root[1]:bbox.top+bbox.height/2;
  const illustrationBox={left:Math.max(0,mx-4),top:Math.max(0,my-4),width:Math.min(w-1,mxx+4)-Math.max(0,mx-4)+1,height:Math.min(h-1,myy+4)-Math.max(0,my-4)+1};
  entries.push({name:definition.name,kind:isLeaf?'leaf':'stone',stage:isLeaf?3:definition.stage,order:isLeaf?100+i:definition.order,
    sprite:`Assets/Resources/Sprite/Home/AltarBuild/Individual/${sprite}`,assembledSprite:`Assets/Resources/Sprite/Home/AltarBuild/Individual/${assembled}`,
    x:bbox.left,y:bbox.top,width:bbox.width,height:bbox.height,pivotX:(pivotPx-bbox.left)/bbox.width,pivotY:1-(pivotPy-bbox.top)/bbox.height,
    offsetX:pivotPx-585,offsetY:36-pivotPy,startOffsetY:isLeaf?0:definition.stage===0?88:definition.stage===1?112:140,visiblePixels,restoredPixels,illustrationBox});
  buffers.push({data:full,definition});
}
// 実際に保持する演出Spriteを描画順で合成し、後続物体が補完面を覆うことを検証する。
for(const {data} of buffers)for(let p=0;p<pixelCount;p++){
  const b=p*4,a=data[b+3]/255,back=fullComposite[b+3]/255,out=a+back*(1-a);
  if(!a)continue;
  for(let c=0;c<3;c++)fullComposite[b+c]=Math.round((data[b+c]*a+fullComposite[b+c]*back*(1-a))/out);
  fullComposite[b+3]=Math.round(out*255);
}
let retainedRgbaDiff=0;for(let i=0;i<source.length;i++)if(source[i]!==fullComposite[i]&&((i%4===3)||source[i-(i%4)+3]>0))retainedRgbaDiff++;
if(retainedRgbaDiff)throw new Error(`保持する演出Spriteの合成が元PNGと異なる: ${retainedRgbaDiff} channels`);
await sharp(fullComposite,{raw:{width:w,height:h,channels:4}}).png().toFile(path.join(qa,'retained-altar.png'));
let rgbaDiff=0;for(let i=0;i<source.length;i++)if(source[i]!==assembledComposite[i]&&((i%4===3)||source[i-(i%4)+3]>0))rgbaDiff++;
if(rgbaDiff)throw new Error(`完成合成が元PNGと異なる: ${rgbaDiff} channels`);
await sharp(assembledComposite,{raw:{width:w,height:h,channels:4}}).png().toFile(path.join(qa,'assembled.png'));
const cellW=192,cellH=176,cols=6,rows=Math.ceil(entries.length/cols);let tiles=[];
for(let i=0;i<entries.length;i++){
  const e=entries[i];const box=e.illustrationBox;const tile=await sharp(buffers[i].data,{raw:{width:w,height:h,channels:4}}).extract(box).resize({width:cellW-24,height:cellH-32,fit:'inside'}).extend({top:12,bottom:12,left:12,right:12,background:{r:0,g:0,b:0,alpha:0}}).png().toBuffer();
  tiles.push({input:tile,left:(i%cols)*cellW,top:Math.floor(i/cols)*cellH});
}
await sharp({create:{width:cellW*cols,height:cellH*rows,channels:4,background:'#eeece5'}}).composite(tiles).png().toFile(path.join(qa,'parts-contact.png'));
await sharp({create:{width:cellW*cols,height:cellH*Math.ceil(stones.length/cols),channels:4,background:{r:0,g:0,b:0,alpha:0}}}).composite(tiles.slice(0,stones.length)).png().toFile(path.join(qa,'stones-contact.png'));
const manifest={source:'Assets/Resources/Sprite/ChatGPT 画像 2026年10月4日 12_40_51.png',sourceWidth:w,sourceHeight:h,crop:{x:42,y:36,width:1086,height:1290},stoneCount:stones.length,leafCount:leaves.length,rgbaDiff,retainedRgbaDiff,uncontainedPixelCount:uncontained,parts:entries};
fs.writeFileSync(path.join(output,'parts.json'),JSON.stringify(manifest,null,2)+'\n');
console.log({stoneCount:stones.length,leafCount:leaves.length,rgbaDiff,retainedRgbaDiff,uncontained,restoredPixels:entries.reduce((n,e)=>n+e.restoredPixels,0)});
