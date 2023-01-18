
/*URL Sidebar dinamikliği*/
var sideBarLink = document.querySelectorAll(".sidebar-wrapper .nav .nav-item");
var uriArray = window.location.href.split("/");

if (window.location.href.includes("/Admin/Index")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[0].classList.add("active");
}


if (window.location.href.includes("/Admin")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[0].classList.add("active");
}

if (window.location.href.includes("/Admin/Students")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[1].classList.add("active");
}

if (window.location.href.includes("/Admin/StudentAdd")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[1].classList.add("active");
}

if (window.location.href.includes("/Admin/StudentUpdate")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[1].classList.add("active");
}

if (window.location.href.includes("/Admin/Teachers")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[2].classList.add("active");
}

if (window.location.href.includes("/Admin/TeacherAdd")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[2].classList.add("active");
}

if (window.location.href.includes("/Admin/TeacherUpdate")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[2].classList.add("active");
}

if (window.location.href.includes("/Admin/AdminProfile")) {
    $(".sidebar-wrapper .nav .nav-item").removeClass("active");
    sideBarLink[3].classList.add("active");
}