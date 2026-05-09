const darkToggle = document.getElementById("darkToggle");

if (localStorage.getItem("darkMode") === "true") {
    document.body.classList.add("dark");
    if (darkToggle) darkToggle.textContent = "Light Mode";
}

if (darkToggle) {
    darkToggle.addEventListener("click", () => {
        document.body.classList.toggle("dark");
        const isDark = document.body.classList.contains("dark");
        darkToggle.textContent = isDark ? "Light Mode" : "Dark Mode";
        localStorage.setItem("darkMode", isDark);
    });
}