// Unityで記録した実フレームを、確認用GIFと状態集計へまとめる。
import fs from 'node:fs';
import path from 'node:path';
import {createRequire} from 'node:module';
const require=createRequire(process.env.CODEX_NODE_MODULES ? path.join(process.env.CODEX_NODE_MODULES,'_entry.cjs') : import.meta.url);
const sharp=require('sharp');
const project=path.resolve(path.dirname(new URL(import.meta.url).pathname.replace(/^\/(?:([A-Za-z]:))/, '$1')), '../..');
const folder=path.join(project,'Temp/HomeEntrance');
const captureFolder='RetainedPlay';
const frames=JSON.parse(fs.readFileSync(path.join(folder,captureFolder,'trace.json'))).frames;
const selected=frames.filter((f,i)=>i%2===0||i===frames.length-1);
const width=320,height=569;
const raw=[];
for(const f of selected) raw.push(await sharp(path.join(folder,`${captureFolder}/frame-${String(f.index).padStart(4,'0')}.png`)).resize(width,height).ensureAlpha().raw().toBuffer());
const delay=selected.map((f,i)=>Math.max(20,Math.round(((selected[i+1]?.time??f.time+.4)-f.time)*1000)));
await sharp(Buffer.concat(raw),{raw:{width,height:height*raw.length,channels:4,pageHeight:height}}).gif({delay,loop:0,effort:7}).toFile(path.join(folder,'home-entrance-preview.gif'));
const stages=[frames[0],frames.find(f=>f.stoneCount>=10),frames.find(f=>f.stoneCount===37&&f.leafCount===0),frames.find(f=>f.leafCount===37&&f.propCount===0),frames.find(f=>f.propCount===2&&f.propLeafCount===0),frames.find(f=>f.propLeafCount>=8),frames.find(f=>f.completedPropLeaves===16),frames.at(-1)].map(f=>f??frames.at(-1));
const tiles=[];
for(let i=0;i<stages.length;i++)tiles.push({input:await sharp(path.join(folder,`${captureFolder}/frame-${String(stages[i].index).padStart(4,'0')}.png`)).resize(240,427).png().toBuffer(),left:i%4*240,top:Math.floor(i/4)*427});
await sharp({create:{width:960,height:854,channels:4,background:'#24262a'}}).composite(tiles).png().toFile(path.join(folder,'home-entrance-contact.png'));
const report={frameCount:frames.length,recordedSeconds:frames.at(-1).time,firstMain:frames.find(f=>f.scene==='MainScene')?.time,firstLeaf:frames.find(f=>f.leafCount>0)?.time,firstProp:frames.find(f=>f.propCount>0)?.time,firstPropLeaf:frames.find(f=>f.propLeafCount>0)?.time,allPropLeaves:frames.find(f=>f.completedPropLeaves===16)?.time,homeReady:frames.find(f=>f.scene==='MainScene'&&f.hudAlpha===1&&!f.loadingExists)?.time,maxWhiteFraction:Math.max(...frames.map(f=>f.whiteFraction)),maxBlackFraction:Math.max(...frames.map(f=>f.blackFraction)),maxYellowFraction:Math.max(...frames.map(f=>f.yellowFraction)),blankHomeFrames:frames.filter(f=>f.scene==='MainScene'&&!f.homeBackground).length,earlyLoadingEndFrames:frames.filter(f=>f.scene==='MainScene'&&f.completedPropLeaves<16&&(!f.loadingExists||f.loadingAlpha<1)).length,earlyInteractiveFrames:frames.filter(f=>f.scene==='MainScene'&&f.hudAlpha<1&&f.interactableProps>0).length,visibleStaticAltarFrames:frames.filter(f=>f.staticAltar).length,visibleLegacyPropFrames:frames.filter(f=>f.visibleLegacyProps>0).length,maxSpriteChangedCount:Math.max(...frames.map(f=>f.spriteChangedCount)),maxPartObjectChangedCount:Math.max(...frames.map(f=>f.partObjectChangedCount)),maxPropLeafRootError:Math.max(...frames.map(f=>f.propLeafRootError)),finalPartCount:frames.at(-1).totalParts,finalInteractiveProps:frames.at(-1).interactableProps};
for(const [field,left,right] of [['renderComparison','assembled-render.png','static-render.png'],['propRenderComparison','retained-props-render.png','original-props-render.png']])if(fs.existsSync(path.join(folder,left))){
  const a=await sharp(path.join(folder,left)).raw().toBuffer();
  const b=await sharp(path.join(folder,right)).raw().toBuffer();
  let sum=0,max=0,changed=0;
  for(let p=0;p<a.length/3;p++){let d=0;for(let c=0;c<3;c++){const delta=Math.abs(a[p*3+c]-b[p*3+c]);sum+=delta;max=Math.max(max,delta);d=Math.max(d,delta);}if(d>3)changed++;}
  report[field]={meanChannelDifference:sum/a.length,maxChannelDifference:max,changedPixelsOver3:changed};
}
fs.writeFileSync(path.join(folder,'qa-summary.json'),JSON.stringify(report,null,2)+'\n');
console.log(report);
