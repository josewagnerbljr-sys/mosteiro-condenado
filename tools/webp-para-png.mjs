import { createRequire } from 'module';
const require = createRequire('/home/claude/.npm-global/lib/node_modules/@gltf-transform/cli/');
const { NodeIO } = require('@gltf-transform/core');
const { ALL_EXTENSIONS } = require('@gltf-transform/extensions');
const sharp = require('sharp');
const [,, entrada, saida] = process.argv;
const io = new NodeIO().registerExtensions(ALL_EXTENSIONS);
const doc = await io.read(entrada);
for (const tex of doc.getRoot().listTextures()) {
  if (tex.getMimeType() === 'image/webp') {
    const buf = await sharp(Buffer.from(tex.getImage())).png().toBuffer();
    tex.setImage(new Uint8Array(buf)).setMimeType('image/png');
  }
}
for (const ext of doc.getRoot().listExtensionsUsed()) if (ext.extensionName === 'EXT_texture_webp') ext.dispose();
await io.write(saida, doc);
