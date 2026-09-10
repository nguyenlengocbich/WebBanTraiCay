$(document).ready(function () {
    $('#myTable').DataTable({
        autoWidth: false,
        searching: false,
        responsive: true,
        lengthChange: true,
        paging: true,
        order: [[0, 'asc']],
        columnDefs: [
            { targets: 'col-actions', orderable: false, searchable: false, width: '140px', responsivePriority: 1 },
            { targets: [3, 4], className: 'dt-body-right dt-head-right', responsivePriority: 2 }, // số lượng, đơn giá
            { targets: [6], orderable: false, searchable: false, responsivePriority: 3 } // ảnh
        ]
    });
});