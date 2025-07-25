import { Link } from "react-router-dom";

function SideBar() {
  return (
    <div style={{
      width: "200px",
      height: "100vh",
      background: "#f0f0f0",
      padding: "20px",
      boxSizing: "border-box"
    }}>
      <nav>
        <ul style={{ listStyle: "none", padding: 0 }}>
          <li style={{ marginBottom: "10px" }}>
            <Link to="/">Main Page</Link>
          </li>
          <li>
            <Link to="/crossroad">Crossroad</Link>
          </li>
        </ul>
      </nav>
    </div>
  );
}

export default SideBar;
