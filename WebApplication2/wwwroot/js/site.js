$(document).ready(function () {
    $('#userDropdown').click(function () {
        $('#userDropdownMenu').toggle();
    });

    $(".logoutBtn").click(function () {
        // Xóa cookie xác thực từ client-side
        document.cookie = '.AspNetCore.Cookies=; expires=Thu, 01 Jan 1970 00:00:01 GMT; path=/;';

        // Chuyển hướng đến trang đăng nhập sau khi đăng xuất thành công
        window.location.href = '/user/Login';
    });

    function getUnreadCount() {
        // Thực hiện yêu cầu AJAX để lấy số lượng thông báo chưa đọc từ máy chủ
        $.get("/Notification/GetUnreadCount", function (data) {
            // Cập nhật giá trị của phần tử <span> với số lượng thông báo chưa đọc
            $("#unreadCount").text(data);
        });
    }

    // Gọi hàm để cập nhật số lượng thông báo chưa đọc khi trang được tải
    getUnreadCount();

    // Xử lý sự kiện click trên biểu tượng chuông để cập nhật lại số lượng thông báo chưa đọc
    $(".nav-link").click(function () {
        getUnreadCount();
    });

    document.getElementById("searchInput").addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            var searchString = this.value.trim();
            if (searchString !== "") {
                // Thay đổi URL khi nhấn Enter
                window.location.href = "/UserPost/CombinedSearch?searchString=" + encodeURIComponent(searchString);
            }
            event.preventDefault(); // Ngăn chặn hành động mặc định của nút Enter
        }
    });
});
$(".like-btn").click(function () {
    var postid = $(this).data("postid");
    var likeCount = $("#likes-count-" + postid);

    $.ajax({
        type: "POST",
        url: "/like/" + postid,
        success: function (data) {
            likeCount.text(data.likes);
        },
        error: function () {
            alert("Đã xảy ra lỗi khi like bài đăng.");
        }
    });
});
document.getElementById('textInput').addEventListener('click', function () {
    window.location.href = '/UserPost/Create';
});
$(".save-post-btn").click(function () {
    var postId = $(this).data("postid");

    // Gửi yêu cầu lưu bài đến server
    $.ajax({
        type: "POST",
        url: "/UserPost/SavePost",
        data: { postId: postId },
        success: function (response) {
            alert(response); // Hiển thị thông báo từ server
        },
        error: function (response) {
            alert("response");
        }
    });
});


$(document).ready(function () {
    $(".zoomable-image").click(function () {
        var img = $(this).clone();
        showModal(img);
    });

    $(document).on("click", ".image-modal", function (e) {
        if (e.target == this) {
            hideModal();
        }
    });
});

let modal;
function showModal(img) {
    if (!modal) {
        modal = $("<div>", { "class": "image-modal" });
        $("body").append(modal);
    }

    var modalContent = $("<img>", { "class": "image-modal-content", "src": img.attr("src") });
    modal.html(modalContent);
    modal.show();
}

function hideModal() {
    if (modal) {
        modal.hide();
    }
}
const contentElements = document.querySelectorAll('.content-text');

contentElements.forEach(contentElement => {
    const fullText = contentElement.dataset.fullText;
    const truncatedText = contentElement.textContent;

    if (fullText.length > 132) {
        contentElement.classList.add('text-truncated');
        contentElement.setAttribute('title', fullText); // Thêm title để hiển thị đoạn văn bản đầy đủ khi hover

        contentElement.addEventListener('click', () => {
            if (contentElement.classList.contains('text-truncated')) {
                contentElement.textContent = fullText;
                contentElement.classList.remove('text-truncated');
            } else {
                contentElement.textContent = truncatedText;
                contentElement.classList.add('text-truncated');
            }
        });
    }
});