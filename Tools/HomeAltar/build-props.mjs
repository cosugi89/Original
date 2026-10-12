// 小道具の植物を葉ごとに分け、補完面→葉→既存可視面の重なりで同じSpriteを保持する。
import fs from 'node:fs';
import path from 'node:path';
import {createRequire} from 'node:module';
const require=createRequire(process.env.CODEX_NODE_MODULES ? path.join(process.env.CODEX_NODE_MODULES,'_entry.cjs') : import.meta.url);
const sharp=require('sharp');
const project=path.resolve(path.dirname(new URL(import.meta.url).pathname.replace(/^\/(?:([A-Za-z]:))/, '$1')), '../..');
const out=path.join(project,'Assets/Resources/Sprite/Home/AltarBuild/Individual');
const qa=path.join(project,'Temp/HomeEntrance');
const definitions=JSON.parse(fs.readFileSync(path.join(project,'Tools/HomeAltar/props-leaves.json'))).props;
const parts=[],reports=[];
function contains(poly,x,y){let inside=false;for(let i=0,j=poly.length-1;i<poly.length;j=i++){const [ax,ay]=poly[i],[bx,by]=poly[j];if((ay>y)!==(by>y)&&x<(bx-ax)*(y-ay)/(by-ay)+ax)inside=!inside;}return inside;}
function segmentDistance(l,x,y){const [ax,ay]=l.root,[bx,by]=l.tip,vx=bx-ax,vy=by-ay;const t=Math.max(0,Math.min(1,((x-ax)*vx+(y-ay)*vy)/(vx*vx+vy*vy)));return (x-ax-t*vx)**2+(y-ay-t*vy)**2;}
function outlineDistance(poly,x,y){let distance=Infinity;for(let i=0;i<poly.length;i++)distance=Math.min(distance,segmentDistance({root:poly[i],tip:poly[(i+1)%poly.length]},x,y));return distance;}
for(let pi=0;pi<definitions.length;pi++){
  const prop=definitions[pi],{data:source,info}=await sharp(path.join(project,prop.source)).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const w=info.width,h=info.height,n=w*h;
  const restored=await sharp(path.join(project,`Tools/HomeAltar/${prop.name.toLowerCase()}-restored-underlay.png`)).resize(w,h,{fit:'fill'}).ensureAlpha().raw().toBuffer();
  const owners=new Int16Array(n).fill(-1);
  const region=new Uint8Array(n);
  const nearRegion=new Uint8Array(n);
  const bodyRegion=new Uint8Array(n);
  for(let p=0;p<n;p++)if(prop.bodyExclusionPolygons.some(poly=>contains(poly,p%w+.5,Math.floor(p/w)+.5)))bodyRegion[p]=1;
  for(let p=0;p<n;p++)if(prop.leaves.some(l=>contains(l.polygon,p%w+.5,Math.floor(p/w)+.5)))region[p]=1;
  // 指定多角形は葉の緑面に沿う。太い輪郭とアンチエイリアスを含む自然な外形まで探索する。
  for(let p=0;p<n;p++)if(region[p]){
    const x=p%w,y=Math.floor(p/w);
    for(let dy=-40;dy<=40;dy++)for(let dx=-40;dx<=40;dx++)if(dx*dx+dy*dy<=1600&&x+dx>=0&&x+dx<w&&y+dy>=0&&y+dy<h)nearRegion[(y+dy)*w+x+dx]=1;
  }
  const seeds=new Uint8Array(n);
  for(let p=0;p<n;p++){
    const b=p*4,x=p%w,y=Math.floor(p/w),r=source[b],g=source[b+1],blue=source[b+2];
    if(!source[b+3]||!nearRegion[p])continue;
    // 淡い黄緑のハイライトも葉に含める。補描後に透明になる外周は黒い輪郭ごと採る。
    const green=g>r*1.005&&g>blue*1.12&&g-r>1;
    if(bodyRegion[p]&&!(region[p]&&green))continue;
    // 補描済み本体に重なる暗色は木・石の固定輪郭として保持する。
    if(!green&&restored[b+3]>=128)continue;
    if(!green&&restored[b+3]>10)continue;
    seeds[p]=1;
  }
  // 緑の面に付いた太い暗色輪郭を回収する。小道具の苔・金装飾は葉領域の外にある。
  for(let p=0;p<n;p++){
    const b=p*4,x=p%w,y=Math.floor(p/w);if(source[b+3]<16)continue;
    let isLeaf=seeds[p]===1;
    if(!isLeaf&&!bodyRegion[p]&&restored[b+3]<128&&nearRegion[p]&&Math.max(source[b],source[b+1],source[b+2])<128){
      for(let dy=-7;dy<=7&&!isLeaf;dy++)for(let dx=-7;dx<=7;dx++)if(dx*dx+dy*dy<=49&&x+dx>=0&&x+dx<w&&y+dy>=0&&y+dy<h&&seeds[(y+dy)*w+x+dx]){isLeaf=true;break;}
    }
    if(!isLeaf)continue;
    let distance=Infinity,index=-1;
    // 重なった根元は描画順を優先し、外側の輪郭は葉の実形に最も近い領域へ帰属させる。
    for(let i=0;i<prop.leaves.length;i++)if(contains(prop.leaves[i].polygon,x+.5,y+.5))index=i;
    if(index<0)for(let i=0;i<prop.leaves.length;i++){const d=outlineDistance(prop.leaves[i].polygon,x,y);if(d<distance){distance=d;index=i;}}
    owners[p]=index;
  }
  const visible=Buffer.alloc(source.length),underlay=Buffer.alloc(source.length),leafBuffers=prop.leaves.map(()=>Buffer.alloc(source.length));
  for(let p=0;p<n;p++){
    const b=p*4;if(owners[p]<0)source.copy(visible,b,b,b+4);
    else{
      source.copy(leafBuffers[owners[p]],b,b,b+4);
      // 葉のalphaが1になれば必ず覆われる部分だけ補完する。透明な外形へ材料をはみ出させない。
      if(source[b+3]>=250&&restored[b+3]>0)restored.copy(underlay,b,b,b+4);
    }
  }
  const baseName=`Prop_${prop.name}`;
  const rootSprite=`Assets/Resources/Sprite/Home/AltarBuild/Individual/${baseName}_Underlay.png`;
  const bodySprite=`Assets/Resources/Sprite/Home/AltarBuild/Individual/${baseName}_Visible.png`;
  const crop={left:prop.crop.x,top:prop.crop.y,width:prop.crop.width,height:prop.crop.height};
  await sharp(underlay,{raw:{width:w,height:h,channels:4}}).extract(crop).png().toFile(path.join(project,rootSprite));
  await sharp(visible,{raw:{width:w,height:h,channels:4}}).extract(crop).png().toFile(path.join(project,bodySprite));
  parts.push({name:baseName,kind:'prop',reference:prop.name,sprite:rootSprite,bodyVisibleSprite:bodySprite,stage:4,order:200+pi,width:crop.width,height:crop.height,pivotX:.5,pivotY:1,offsetX:0,offsetY:0,startOffsetY:120});
  for(let i=0;i<prop.leaves.length;i++){
    const l=prop.leaves[i],data=leafBuffers[i];let minX=l.root[0],maxX=minX,minY=l.root[1],maxY=minY,pixels=0;
    for(let p=0;p<n;p++)if(data[p*4+3]){const x=p%w,y=Math.floor(p/w);minX=Math.min(minX,x);maxX=Math.max(maxX,x);minY=Math.min(minY,y);maxY=Math.max(maxY,y);pixels++;}
    if(!pixels)throw new Error(`空の葉: ${l.name}`);
    const box={left:minX-2,top:minY-2,width:maxX-minX+5,height:maxY-minY+5};
    const name='PropLeaf_'+l.name.replace(/^Leaf_/,'');
    const sprite=`Assets/Resources/Sprite/Home/AltarBuild/Individual/${name}.png`;
    await sharp(data,{raw:{width:w,height:h,channels:4}}).extract(box).png().toFile(path.join(project,sprite));
    parts.push({name,kind:'propLeaf',reference:prop.name,sprite,stage:5,order:220+pi*20+l.order,width:box.width,height:box.height,pivotX:(l.root[0]-box.left)/box.width,pivotY:1-(l.root[1]-box.top)/box.height,offsetX:l.root[0]-(crop.left+crop.width/2),offsetY:crop.top-l.root[1],startOffsetY:0,visiblePixels:pixels});
  }
  const withoutLeaves=await sharp(underlay,{raw:{width:w,height:h,channels:4}}).composite([{input:visible,raw:{width:w,height:h,channels:4}}]).png().toBuffer();
  const composite=Buffer.from(underlay);
  for(const layer of [...leafBuffers,visible])for(let p=0;p<n;p++){
    const b=p*4,a=layer[b+3]/255,back=composite[b+3]/255,outA=a+back*(1-a);if(!a)continue;
    for(let c=0;c<3;c++)composite[b+c]=Math.round((layer[b+c]*a+composite[b+c]*back*(1-a))/outA);
    composite[b+3]=Math.round(outA*255);
  }
  let diff=0,maxDifference=0;for(let b=0;b<source.length;b++)if(source[b]!==composite[b]&&(b%4===3||source[b-b%4+3]>0)){diff++;maxDifference=Math.max(maxDifference,Math.abs(source[b]-composite[b]));}
  // 元画像の葉はalpha250～254も含むため、補完面の重なりによる最大5/255の差を許容する。
  if(maxDifference>5)throw new Error(`${prop.name}完成合成の最大channel差: ${maxDifference}`);
  await sharp(composite,{raw:{width:w,height:h,channels:4}}).png().toFile(path.join(qa,`${prop.name.toLowerCase()}-retained.png`));
  fs.writeFileSync(path.join(qa,`${prop.name.toLowerCase()}-without-leaves.png`),withoutLeaves);
  reports.push({name:prop.name,leafCount:prop.leaves.length,retainedRgbaDiff:diff,maxChannelDifference:maxDifference});
}
fs.writeFileSync(path.join(out,'props.json'),JSON.stringify({leafCount:definitions.reduce((n,p)=>n+p.leaves.length,0),reports,parts},null,2)+'\n');
console.log(reports);
