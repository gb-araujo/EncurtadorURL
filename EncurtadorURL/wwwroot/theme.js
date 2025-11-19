const body = document.body;
const toggleBtn = document.getElementById("themeToggle");

const savedTheme = localStorage.getItem("theme");
if (savedTheme === "dark") {
  body.classList.add("dark");
  toggleBtn.textContent = "Modo Claro";
}

toggleBtn.addEventListener("click", () => {
  body.classList.toggle("dark");

  const isDark = body.classList.contains("dark");

  toggleBtn.textContent = isDark ? "Modo Claro" : "Modo Escuro";

  localStorage.setItem("theme", isDark ? "dark" : "light");
});
