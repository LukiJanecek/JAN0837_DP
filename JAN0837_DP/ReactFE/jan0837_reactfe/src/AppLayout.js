import "./AppLayout.css";

export default function AppLayout({header, left, right, footer, children, showLeft = true, showRight = true})
{
    return (
        <div className="appGrid" data-left={showLeft ? "on" : "off"} data-right={showRight ? "on" : "off"}>
            <header className="header">{header ?? <DefaultHeader/>}</header>
            <aside className="left sidebar">{showLeft ? left : null}</aside>
            <main className="main">{children}</main>
            <aside className="right sidebar">{showRight ? right : null}</aside>
            <footer className="footer">{footer ?? <DefaultFooter/>}</footer>
        </div>
    );
}

function DefaultHeader(){ return <div style={{padding:8, fontWeight:600}}>My App</div>; }
function DefaultFooter(){ return <div style={{padding:8, opacity:.7}}>© 2025</div>; }