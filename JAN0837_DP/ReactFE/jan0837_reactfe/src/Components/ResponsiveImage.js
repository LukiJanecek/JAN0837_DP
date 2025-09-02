const getBase = () => {
  const vite = (typeof import.meta !== 'undefined' && import.meta.env && import.meta.env.BASE_URL) || '';
  const cra  = (typeof process !== 'undefined' && process.env && process.env.PUBLIC_URL) || '';
  const base = vite || cra || '/';
  return base.endsWith('/') ? base.slice(0, -1) : base; 
};

export default function ResponsiveImage({name, ext = 'png', folder = 'images', alt, aspect = "16 / 9", /* např. "4 / 3", "1 / 1"*/ fit = "contain", /*"contain" nebo "cover"*/ radius = 12, style = {}})
{
    const base = getBase();
    const cleanFolder = folder.replace(/^\/+/, ''); 
    const src = `${base}/${cleanFolder}/${name}.${ext}`;

    return (
        <div style={{position: "relative", width: "100%", aspectRatio: aspect, borderRadius: radius, overflow: "hidden", border: "1px solid #e5e7eb", ...style}}>
        <img src={src} alt={alt ?? name} style={{position: "absolute", inset: 0, width: "100%", height: "100%", objectFit: fit}}/>
        </div>
    );
}